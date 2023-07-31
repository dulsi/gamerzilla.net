import sqlite3
from flask import Flask,jsonify,request,make_response
from flask_cors import CORS
app = Flask(__name__)
app.config['CORS_HEADERS'] = 'Content-Type'
app.config['CORS_SUPPORTS_CREDENTIALS'] = True
cors = CORS(app)

boolean_keys = ['admin', 'visible', 'approved']

def get_user_db_connection():
    conn = sqlite3.connect('../db/User.db')
    conn.row_factory = sqlite3.Row
    return conn

def get_trophy_db_connection():
    conn = sqlite3.connect('../db/Trophy.db')
    conn.row_factory = sqlite3.Row
    return conn

def dict_from_row(row):
    answer = {}
    for key in row.keys():
        key = key.replace(key[0], key[0].lower(), 1)
        if key in boolean_keys:
            answer[key] = True if row[key] == 1 else False
        elif key == 'password':
            answer[key] = ''
        elif key != 'sortfield':
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
        users.append(dict_from_row(r))
    conn.close()
    return jsonify(users)

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

@app.route("/api/gamerzilla/game")
def game_data():
    user = request.args["username"]
    user_id = find_user(user)
    params = { "USERID" : user_id, "GAME" : request.args["game"]}
    conn = get_trophy_db_connection()
    r = conn.execute("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME", params).fetchone()
    answer = { "shortname" : r["shortname"], "name" : r["gamename"], "version": r["versionnum"], "trophy": [] }
    params["GAME"] = r["id"]
    cur = conn.execute("select t.trophyname as trophy_name, t.trophydescription as trophy_desc, t.maxprogress as max_progress, IFNULL(s.achieved, 0) as achieved, s.progress from trophy t left outer join userstat s on t.gameid = s.gameid and t.id = s.trophyid and s.userid = :USERID where t.gameid = :GAME", params)
    for r in cur.fetchall():
        answer["trophy"].append(dict_from_row(r))
    conn.close()
    return jsonify(answer)

@app.route("/api/gamerzilla/game/image/show")
def game_image_show():
    conn = get_trophy_db_connection()
    params = { "NAME" : request.args["game"] }
    result = conn.execute("select data from image, game where image.gameid = game.id and game.shortname = :NAME and image.trophyid = -1", params).fetchone()
    response = make_response(result["data"])
    response.headers.set('Content-Type', 'image/png')
    return response

@app.route("/api/gamerzilla/trophy/image/show")
def game_trophy_image_show():
    conn = get_trophy_db_connection()
    params = { "NAME" : request.args["game"], "TROPHY" : request.args["trophy"], "ACHIEVED" : request.args["achieved"] }
    result = conn.execute("select data from image i, game g, trophy t where i.gameid = g.id and g.shortname = :NAME and i.trophyid = t.id and g.id = t.gameid and t.trophyname = :TROPHY and i.achieved = :ACHIEVED", params).fetchone()
    response = make_response(result["data"])
    response.headers.set('Content-Type', 'image/png')
    return response
