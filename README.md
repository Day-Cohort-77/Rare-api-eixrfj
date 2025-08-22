# Rare Publishing Platform Notes

Our client, Rare Publishing, needs a new application built for their readers. Currently, readers can submit new articles through the mail and once month Rare sends out a Zine of articles that the publishers liked the most. They've finally decided that the internet is not a fad and want a new way for readers view posts.

The finished application will give users the ability to submit, update and comment on posts. The posts will also be organized by tags and categories making it easier for the reader to find the posts they are searching for.

The previous dev team was able to complete the client and server side portions of login and register. It is up to you to complete the remaining tickets. It is also up to you to decide how many of those tickets you will complete in the first sprint.

Take a few hours to look over the remaining tickets, ERD, and wireframes. Think about what needs to be completed on the server and client side for each ticket to be considered "Done". Write any notes you might have for each ticket as needed in github and ask any clarifying questions you have.

Once you have an idea of how many tickets your group can complete this sprint let your Product Manager know.

# Setup

Run the following commands in the terminal once you clone the project.

Initialize secrets

```sh
dotnet user-secrets init
```

Create the connection string with the following command after you change the password value.

```sh
dotnet user-secrets set 'ConnectionStrings:RareConnectionString' 'Host=localhost;Port=5432;Username=postgres;Password<your_postgres_password;Database=rare'
```

## ERD

The database development team has already taken a stab at the [ERD for this project](https://drawsql.app/nss-2/diagrams/rare-v1). You will use this to start building the project.


## Wireframes

Wireframes from Product Team
https://miro.com/app/board/o9J_kiGCSK4=/