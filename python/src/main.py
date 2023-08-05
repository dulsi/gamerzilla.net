import sqlite3
import json
from flask import Flask,jsonify,request,make_response,session,abort
from flask_cors import CORS
from flask_session import Session
app = Flask(__name__)
app.config['CORS_HEADERS'] = 'Content-Type'
app.config['CORS_SUPPORTS_CREDENTIALS'] = True
cors = CORS(app)
app.config["SESSION_PERMANENT"] = False
app.config["SESSION_TYPE"] = "filesystem"
Session(app)

boolean_keys = ['admin', 'visible', 'approved']

def get_user_db_connection():
    conn = sqlite3.connect('../db/User.db')
    conn.row_factory = sqlite3.Row
    return conn

def get_trophy_db_connection():
    conn = sqlite3.connect('../db/Trophy.db')
    conn.row_factory = sqlite3.Row
    return conn

def is_authorized():
    if 'id' in session:
        return True
    else:
        return False

def authorize(username, password, requireApproved):
    conn = get_user_db_connection()
    args = { "NAME" : username, "PASSWORD" : password }
    r = conn.execute("select id, approved from user u where u.username = :NAME and u.password = :PASSWORD", args).fetchone()
    answer = 0
    if r != None:
        if (requireApproved == 0) or (r["approved"] == 1):
            answer = r["id"]
    conn.close()
    return answer

def addUserStat(conn, gameid, userid, trophyid, achieved, progress):
    params = { "GAME" : gameid, "USERID" : userid, "TROPHY" : trophyid, "ACHIEVED" : achieved, "PROGRESS" : progress }
    conn.execute("insert into userstat(gameid, userid, trophyid, achieved, progress) values (:GAME, :USERID, :TROPHY, :ACHIEVED, :PROGRESS)", params)

def updateUserStat(conn, gameid, userid, trophyid, achieved, progress):
    params = { "GAME" : gameid, "USERID" : userid, "TROPHY" : trophyid, "ACHIEVED" : achieved, "PROGRESS" : progress }
    conn.execute("update userstat set achieved = :ACHIEVED, progress = :PROGRESS WHERE gameid = :GAME and userid = :USERID and trophyid = :TROPHY", params);

def setStat(conn, gameid, userid, trophyid, achieved, progress):
    params = { "GAME" : gameid, "USERID" : userid, "TROPHY" : trophyid }
    orig = conn.execute("select * from userstat where gameid = :GAME and trophyid = :TROPHY and userid = :USERID", params).fetchone()
    if orig == None:
        addUserStat(conn, gameid, userid, trophyid, achieved, progress)
    else:
        params = { "USERSTATID" : orig["id"], "ACHIEVED" : achieved, "PROGRESS" : progress }
        conn.execute("update userstat set achieved = :ACHIEVED, progress = :PROGRESS WHERE gameid = :GAME and userid = :USERID and trophyid = :TROPHY", params);

def dict_from_row(row):
    answer = {}
    for key in row.keys():
        key = key.replace(key[0], key[0].lower(), 1)
        if key in boolean_keys:
            answer[key] = True if row[key] == 1 else False
        elif key == 'password':
            answer[key] = ''
        elif key != 'sortfield' and key != 'id':
            answer[key] = str(row[key])
    return answer

def find_user(user):
    conn = get_user_db_connection()
    args = { "NAME" : user }
    r = conn.execute('select * from user u where u.visible = 1 and u.approved = 1 and username = :NAME', args).fetchone()
    if r != None:
        return r["id"]
    return 0

@app.route("/api/user")
def user_list():
    conn = get_user_db_connection()
    cur = conn.execute('select * from user u where (u.visible = 1 and u.approved = 1)')
    users = []
    for r in cur.fetchall():
        u = dict_from_row(r)
        if "admin" in session and session["admin"]:
            if u["approved"]:
                u["canApprove"] = False
            else:
                u["canApprove"] = True
        else:
            u["admin"] = False
            u["canApprove"] = False
        users.append(u)
    conn.close()
    return jsonify(users)

@app.route("/api/user/login", methods=['POST'])
def user_login():
    userid = authorize(request.get_json().get("username"), request.get_json().get("password"), 0)
    if userid == 0:
        abort(401)
    else:
        session["id"] = userid
        conn = get_user_db_connection()
        args = { "USERID" : userid }
        answer = dict_from_row(conn.execute('select * from user u where u.id = :USERID', args).fetchone())
        if answer["admin"] == 1:
            session["admin"] = True
        else:
            session["admin"] = False
        conn.close()
        return jsonify(answer)

@app.route("/api/user/canregister")
def user_canregister():
    return jsonify(False)

@app.route("/api/user/whoami")
def user_whoami():
    if is_authorized():
        conn = get_user_db_connection()
        args = { "USERID" : session["id"] }
        answer = dict_from_row(conn.execute('select * from user u where u.id = :USERID', args).fetchone())
        conn.close()
        return jsonify(answer)
    else:
        abort(401)

@app.route("/api/gamerzilla/games")
def game_list():
    user = request.args["username"]
    user_id = find_user(user)
    params = { "USERID" : user_id, "LIMIT" : 20, "OFFSET" : 0 }
    conn = get_trophy_db_connection()
    cur = conn.execute("select ShortName as shortname, gamename as name, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = :USERID) order by sortfield desc limit :LIMIT offset :OFFSET", params)
    answer = { "currentPage" : 0, "pageSize": 20, "totalPages" : 1, "games": [] }
    for r in cur.fetchall():
        answer["games"].append(dict_from_row(r))
    conn.close()
    return jsonify(answer)

@app.route("/api/gamerzilla/games", methods=['POST'])
def game_list2():
    user_id = authorize(request.authorization.username, request.authorization.password, 1)
    if user_id == 0:
        abort(401)
    params = { "USERID" : user_id }
    conn = get_trophy_db_connection()
    cur = conn.execute("select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = :USERID) order by sortfield desc", params)
    answer = []
    for r in cur.fetchall():
        answer.append(dict_from_row(r))
    conn.close()
    return jsonify(answer)

@app.route("/api/gamerzilla/game", methods=['GET','POST'])
def game_data():
    if request.method == 'POST':
        user_id = authorize(request.authorization.username, request.authorization.password, 1)
        game = request.form.get("game")
    else:
        user = request.args["username"]
        user_id = find_user(user)
        game = request.args["game"]
    params = { "USERID" : user_id, "GAME" : game}
    conn = get_trophy_db_connection()
    r = conn.execute("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME", params).fetchone()
    answer = { "shortname" : r["shortname"], "name" : r["gamename"], "version": r["versionnum"], "trophy": [] }
    params["GAME"] = r["id"]
    cur = conn.execute("select t.trophyname as trophy_name, t.trophydescription as trophy_desc, t.maxprogress as max_progress, IFNULL(s.achieved, 0) as achieved, s.progress from trophy t left outer join userstat s on t.gameid = s.gameid and t.id = s.trophyid and s.userid = :USERID where t.gameid = :GAME", params)
    for r in cur.fetchall():
        answer["trophy"].append(dict_from_row(r))
    conn.close()
    return jsonify(answer)

@app.route("/api/gamerzilla/game/add", methods=['POST'])
def game_add():
    user_id = authorize(request.authorization.username, request.authorization.password, 1)
    if user_id == 0:
        abort(401)
    game_info = json.loads(request.form["game"])
    conn = get_trophy_db_connection()
    params = { "GAME" : game_info["shortname"] }
    game = conn.execute("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME", params).fetchone()
    authority = 1
    if game != None:
        game_id = game["id"]
        if int(game_info["version"]) > game["versionnum"]:
            params = { "VERSION" : game_info["version"], "ID" : game_id }
            conn.execute("update game set versionnum = :VERSION where id = :ID", params)
            authority = 2
        elif int(game_info["version"]) < game["versionnum"]:
            game_info["version"] = str(game["versionnum"])
    else:
        params = { "SHORT" : game_info["shortname"], "NAME" : game_info["name"], "VERSION" : game_info["version"] }
        conn.execute("insert into game(shortname, gamename, versionnum) values (:SHORT, :NAME, :VERSION)", params);
        params = { "GAME" : game_info["shortname"] }
        game = conn.execute("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME", params).fetchone()
        game_id = game["id"]
    params = { "GAME" : game_id, "USERID" : user_id }
    trophies = conn.execute("select t.id, t.trophyname, t.trophydescription, t.maxprogress, s.achieved, s.progress from trophy t left outer join userstat s on t.gameid = s.gameid and t.id = s.trophyid and s.userid = :USERID where t.gameid = :GAME", params).fetchall()
    used = []
    for t in trophies:
        for t_in in game_info["trophy"]:
            if t["trophyname"] == t_in["trophy_name"]:
                used.append(t["trophyname"])
                if t["trophydescription"] != t_in["trophy_desc"]:
                    if authority == 1:
                        t_in["trophy_desc"] = t["trophydescription"]
                        t_in["max_progess"] = t["maxprogress"]
                    else:
                        params = { "DESC" : t_in["trophy_desc"], "MAX" : t_in["max_progress"], "ID" : t["id"] }
                        conn.execute("update trophy set trophydescription = :DESC, maxprogress = :MAX where id = :ID", params)
                if ("achieved" in t_in and t_in["achieved"] != "0") or ("progress" in t_in and t_in["progress"] != "0"):
                    if t["achieved"] == None and t["progress"] == None:
                        addUserStat(conn, game_id, user_id, t["id"], t_in["achieved"], t_in["progress"])
                    else:
                        updateUserStat(conn, game_id, user_id, t["id"], t_in["achieved"], t_in["progress"])
        if t["trophyname"] not in used:
            game_info["trophy"].append({ "trophy_name" : t["trophyname"], "trophy_desc" : t["trophydescription"], "achieved" : "1" if t["achieved"] == 1 else "0", "progress" : str(t["progress"]) if t.get("progress") != None else "0", "max_progress" : t["maxprogress"] })
    for t_in in game_info["trophy"]:
        if t_in["trophy_name"] not in used:
            params = { "GAME" : game_id, "NAME" : t_in["trophy_name"], "DESC" : t_in["trophy_desc"], "MAX" : t_in["max_progress"] }
            conn.execute("insert into trophy(gameid, trophyname, trophydescription, maxprogress) values (:GAME, :NAME, :DESC, :MAX)", params);
            if ("achieved" in t_in and t_in["achieved"] != "0") or ("progress" in t_in and t_in["progress"] != "0"):
                params = { "GAME" : game_id, "NAME" : t_in["trophy_name"] }
                trophy_id = conn.execute("select id from trophy where gameid = :GAME and trophyname = :NAME", params).fetchone()["id"];
                addUserStat(conn, game_id, user_id, trophy_id, t_in["achieved"], t_in["progress"]);
    conn.commit();
    conn.close()
    return jsonify(game_info)

@app.route("/api/gamerzilla/game/image/show", methods=['GET','POST'])
def game_image_show():
    conn = get_trophy_db_connection()
    if request.method == 'POST':
        params = { "NAME" : request.form["game"] }
    else:
        params = { "NAME" : request.args["game"] }
    result = conn.execute("select data from image, game where image.gameid = game.id and game.shortname = :NAME and image.trophyid = -1", params).fetchone()
    response = make_response(result["data"])
    response.headers.set('Content-Type', 'image/png')
    conn.close()
    return response

@app.route("/api/gamerzilla/trophy/set", methods=['POST'])
def game_trophy_set():
    user_id = authorize(request.authorization.username, request.authorization.password, 1)
    if user_id == 0:
        abort(401)
    conn = get_trophy_db_connection()
    params = { "GAME" : request.form["game"] }
    game_id = conn.execute("select id from game g where g.shortname = :GAME", params).fetchone()["id"]
    params = { "GAME" : game_id, "TROPHY" : request.form["trophy"] }
    trophy_id = conn.execute("select id from trophy where gameid = :GAME and trophyname = :TROPHY", params).fetchone()["id"]
    setStat(conn, game_id, user_id, trophy_id, 1, 0);
    conn.commit()
    conn.close()
    return "OK";

@app.route("/api/gamerzilla/trophy/set/stat", methods=['POST'])
def game_trophy_set_stat():
    user_id = authorize(request.authorization.username, request.authorization.password, 1)
    if user_id == 0:
        abort(401)
    conn = get_trophy_db_connection()
    params = { "GAME" : request.form["game"] }
    game_id = conn.execute("select id from game g where g.shortname = :GAME", params).fetchone()["id"]
    params = { "GAME" : game_id, "TROPHY" : request.form["trophy"] }
    trophy = conn.execute("select id,maxprogress from trophy where gameid = :GAME and trophyname = :TROPHY", params).fetchone()
    progress = int(request.form["progress"])
    setStat(conn, game_id, user_id, trophy["id"], 1 if progress >= trophy["maxprogress"] else 0, progress);
    conn.commit()
    conn.close()
    return "OK";

@app.route("/api/gamerzilla/trophy/image/show", methods=['GET','POST'])
def game_trophy_image_show():
    conn = get_trophy_db_connection()
    if request.method == 'POST':
        params = { "NAME" : request.form["game"], "TROPHY" : request.form["trophy"], "ACHIEVED" : request.form["achieved"] }
    else:
        params = { "NAME" : request.args["game"], "TROPHY" : request.args["trophy"], "ACHIEVED" : request.args["achieved"] }
    result = conn.execute("select data from image i, game g, trophy t where i.gameid = g.id and g.shortname = :NAME and i.trophyid = t.id and g.id = t.gameid and t.trophyname = :TROPHY and i.achieved = :ACHIEVED", params).fetchone()
    response = make_response(result["data"])
    response.headers.set('Content-Type', 'image/png')
    conn.close()
    return response
