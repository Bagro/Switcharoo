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
    Id     INTEGER NOT NULL
        constraint Environments_pk
            primary key autoincrement,
    Key    TEXT    NOT NULL,
    Name   TEXT    NOT NULL,
    UserId INTEGER NOT NULL
        constraint Environments_Users_id_fk
            references Users
);

create table Features
(
    Id          INTEGER NOT NULL
        constraint Features_pk
            primary key autoincrement,
    Key         TEXT    NOT NULL,
    Name        TEXT    NOT NULL,
    Description TEXT    NOT NULL,
    UserId      INTEGER NOT NULL
        constraint Features_Users_id_fk
            references Users
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