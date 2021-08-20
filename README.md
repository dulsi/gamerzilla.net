# Gamerzilla.Net

## Setup on Fedora

Setting up Gamerzilla.Net is currently more complex than we would like.
We appoligize for this situation and will be working to correct this in
the future.

Clone the git repository with:

```
git clone https://github.com/dulsi/gamerzilla.net.git
```

Change to the frontend directory and install the javascript libraries.

```
cd gamerzilla.net/frontend
npm install
```

Edit src/AppSettings.ts. Remove the "+ ':5000'" from the first line.
Remove "${server}" from the last line. Then build the frontend and copy
the files to the web root.

```
npm run build
cp -r build/* /var/www/html
```

For the backend, we need dotnet installed. Then we can build and copy
the published files. (This assumes .net 5. The directory in Release may
change with other .net versions.)

```
dnf install dotnet
mkdir /var/www/gamerzilla.net
cd ../backend
dotnet publish --configuration Release
cp -r bin/Release/net5.0/publish/* /var/www/gamerzilla.net/
```

Setup the .net backend to run as a service.

```
cp gamerzilla.service /etc/systemd/system
restorecon /etc/systemd/system/gamerzilla.service
systemctl daemon-reload
systemctl enable gamerzilla
systemctl start gamerzilla
```

There is some additional configuration needed for apache. Edit
/etc/httpd/conf/httpd.conf. Find the "/var/www/html" directory section.
Change "AlowOverride None" to "AllowOverride All".

```
cp gamerzilla.conf /etc/httpd/conf.d/
/usr/sbin/setsebool -P httpd_can_network_connect 1
systemctl restart httpd
```

At this point the system is running but has no users. Connect to the
website by going to http://localhost/. Click on sign in. Type in
whatever for username and password. That will cause the two sqlite
databases to be created. Go to command line and connect to the User,db:

```
sqlite3 /var/www/gamerzilla.net/User.db
```

Create a user with sql:

```
insert into User(username,password,admin,visible,approved) values ('somename','somepwd', 1, 1, 1);
.quit
```

Currently there is no way to register new users. They must be inserted
directly in the database.

## Setup on other Linux distributions

Other Linux distributions should be largely the same as Fedora. The
package manager may be different and dotnet may or may not be packaged.

## What if you want you don't want it to be the web root?

We have not tested this yet. It should be as simple as updating
src/AppSettings.ts to have the additional path before /api and modifying
the /etc/http/conf.d/gamerzilla.conf rule.
