create table Users
(
    Id      INTEGER NOT NULL
        constraint Users_pk
            primary key autoincrement,
    AuthKey TEXT    NOT NULL,
    Name    TEXT    NOT NULL
);

create table Environments
(
    Id   INTEGER NOT NULL
        constraint Environments_pk
            primary key autoincrement,
    Key  TEXT    NOT NULL,
    Name TEXT    NOT NULL
);

create table UserEnvironment
(
    UserId        INTEGER NOT NULL
        constraint UserEnvironment_Users_authKey_fk
            references Users,
    EnvironmentId INTEGER NOT NULL
        constraint UserEnvironment_Environments_key_fk
            references Environments,
    constraint UserEnvironment_pk
        primary key (UserId, EnvironmentId)
);

create table Features
(
    Id          INTEGER NOT NULL
        constraint Features_pk
            primary key autoincrement,
    Name        TEXT    NOT NULL,
    Description TEXT    NOT NULL
);

create table FeatureEnvironments
(
    FeatureId     INTEGER           NOT NULL
        constraint FeatureEnvironment_Features_id_fk
            references Features,
    EnvironmentId INTEGER           NOT NULL
        constraint FeatureEnvironment_Environments_id_fk
            references Environments,
    Active        INTEGER default 0 NOT NULL,
    constraint FeatureEnvironment_pk
        primary key (FeatureId, EnvironmentId)
);