# Switcharoo
Headless feature toggling system.

## Settings
### Database
Supported databases are Sqlite, PostgreSQL and MariaDB/MySQL. 
The setting DbType is used to specify witch one to use.

#### Sqlite
Example config
```json
"ConnectionStrings": {
  "SwitcharooDb": "Data Source=switcharoo.sqlite"
},
"DbType": "Sqlite"
```

#### PostgreSQL
```json
"ConnectionStrings": {
  "SwitcharooDb": "Host=127.0.0.1;Database=switcharoo;Username=switcharoo;Password=switcharoo"
},
"DbType": "PostgreSQL"
```

#### MariaDB
In addition to connection string and DbType, server version is needed in _MyMariaVersion_
```json
"ConnectionStrings": {
  "SwitcharooDb": "Server=127.0.0.1;Database=switcharoo;Uid=switcharoo;Pwd=switcharoo;"
},
"DbType": "MariaDB",
"MyMariaVersion": "11.2.2"
```
#### MySQL
In addition to connection string and DbType, server version is needed in _MyMariaVersion_
```json
"ConnectionStrings": {
  "SwitcharooDb": "Server=127.0.0.1;Database=switcharoo;Uid=switcharoo;Pwd=switcharoo;"
},
"DbType": "MySQL",
"MyMariaVersion": "8.2.0"
```
### CORS
To support frontends you can add CORS origins the setting _CorsOrigins_.

_Example_
```json
"CorsOrigins": "http://localhost:5173;http://localhost:6173"
```