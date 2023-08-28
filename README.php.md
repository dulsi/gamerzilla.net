# Gamerzilla PHP

## Setup on Fedora

We need some software installed before installing Gamerzilla.Net. The
instructions assume you are running as root. Otherwise you need to use
sudo.

```
dnf install dotnet httpd php php-common php-gd php-fpm sqlite nodejs-npm
```

After installing, you will need to edit /etc/httpd/conf/httpd.conf. Find
the Directory section for /var/www/html. Change AllowOveride from None to
All. The start httpd and enable the firewall to allow requests.

```
systemctl start httpd
systemctl enable httpd
firewall-cmd --permanent --add-service=http
systemctl reload firewalld
```

Clone the git repository with:

```
git clone https://github.com/dulsi/gamerzilla.net.git
```

Copy the php files to the web server files:

```
cd gamerzilla.net
cp -R php /var/www/html/api
```

Create the databases:

```
mkdir /var/www/html/api/db
cp db/.htaccess /var/www/html/api/db/
sqlite3 /var/www/html/api/db/Trophy.db <db/Trophy.sql
sqlite3 /var/www/html/api/db/User.db <db/User.sql
chown -R apache:apache /var/www/html/.
chcon -t httpd_sys_rw_content_t /var/www/html/api/db -R
```

Connect to the User.db:

```
sqlite3 /var/www/html/api/db/User.db
```

Create a user with sql:

```
insert into User(username,password,admin,visible,approved) values ('somename','somepwd', 1, 1, 1);
.quit
```

Change to the frontend directory and install the javascript libraries.

```
cd frontend
npm install
```

Edit src/AppSettings.ts. Remove the "+ ':5000'" from the first line.
Remove "${server}" from the last line. Then build the frontend and copy
the files to the web root.

```
npm run build
mkdir /var/www/html/trophy
cp -r build/* /var/www/html/trophy
cp .htaccess /var/www/html/trophy
```
