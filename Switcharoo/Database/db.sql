create table Users
(
    AuthKey TEXT NOT NULL
        constraint Users_pk
        primary key,
    Name TEXT NOT NULL
);

create table Environments
(
    Key TEXT NOT NULL
        constraint Environments_pk
        primary key,
    Name TEXT NOT NULL
);

create table UserEnvironment
(
    UserAuthKey TEXT NOT NULL
        constraint UserEnvironment_Users_authKey_fk
            references Users,
    EnvironmentKey TEXT NOT NULL
        constraint UserEnvironment_Environments_key_fk
            references Environments,
    constraint UserEnvironment_pk
        primary key (userAuthKey, environmentKey)
);

create table Features
(
    Id INTEGER NOT NULL
        constraint Features_pk
            primary key,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    Active INTEGER default 0 NOT NULL,
    EnvironmentKey TEXT NOT NULL
        constraint Features_Environments_key_fk
            references Environments,
    constraint Features_Environments_key_fk
        foreign key (environmentKey) references Environments
            on update cascade on delete cascade
);