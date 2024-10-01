-- Helge's .sql schema file but instead of dropping tables if they exist, it creates them if they don't exist.
-- It is done this way so that the relations are created the first time the application is run and persist between runs.

create table if not exists user (
  user_id integer primary key autoincrement,
  username string not null,
  email string not null
);

create table if not exists message (
  message_id integer primary key autoincrement,
  author_id integer not null,
  text string not null,
  pub_date integer
);
