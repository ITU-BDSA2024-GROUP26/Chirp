---
title: _Chirp!_ Project Report
subtitle: ITU BDSA 2024 Group `26`
author:
- "Annam Bashir Chaudhry <abch@itu.dk>"
- "Gutti Torvadal <gutt@itu.dk>"
- "Jovana Novovic <jnov@itu.dk>"
- "Torbjørn Elias Rafael Johannsen <toej@itu.dk>"
- "Vicki Hauge Bjørnskov <vbjo@itu.dk>"
numbersections: true
---

# Design and Architecture of _Chirp!_

## Domain model

Here comes a description of our domain model.

Provide an illustration of your domain model. Make sure that it is correct and complete. In case you are using ASP.NET Identity, make sure to illustrate that accordingly.

![Illustration of the _Chirp!_ data model as UML class diagram.](docs/images/domain_model.png)

## Architecture — In the small
Illustrate the organization of your code base. That is, illustrate which layers exist in your (onion) architecture. Make sure to illustrate which part of your code is residing in which layer.
![Illustration of the _Chirp!_ onion architecture.](images/onion.svg)
As the illustration shows, the _Chirp!_ application is organized in an onion architecture.
The onion architecture pattern has the benefits of making the code highly modular. The dependencies go inwards only,
which means that the inner layers are not dependent on the outer layers. This makes it easy to replace layer implementations,
allowing for a high degree of flexibility and testability.

- Core:
  - At the core are the entities of the domain model. As the program uses EF Core, the entities are also tables in an SQLite database
- Second layer:
  - In the second layer, we have our repositories, which are responsible for reading and writing to the database. Each domain model gets one repository. Additionally, we have a DBRepository, which is responsible for general database operations, that are not tied to a specific domain model. Currently, it has two methods, one for seeding the database and one for resetting it.
- Third layer:
  - The CheepService resides in the third layer and is responsible for the business logic of the application. All the razor pages have a reference to an instance of the service.
- The outermost layer: 
  - The presentation layer. It has the razor pages and the controllers. This layer is responsible for tying it all together as well as rendering UI rendering
## Architecture of deployed application
Illustrate the architecture of your deployed application. Remember, you developed a client-server application. Illustrate the server component and to where it is deployed, illustrate a client component, and show how these communicate with each other.
## User activities
![Illutration of the _Chirp!_ User activities.](images/UserActivities_uml.drawio.svg)
The typical scenarios of a user journey through our Chirp! webapplication are displayed in the Diagram above. The potential user journey begins with opening the webapplication as a non-authorized user being only able to visit the public timeline, visit users from public timelines pages, login as an authorized user and register as such. In addition to the user journey in the illustration, the user can logout of the application from every page. 

## Sequence of functionality/calls trough _Chirp!_
With a UML sequence diagram, illustrate the flow of messages and data through your Chirp! application. Start with an HTTP request that is send by an unauthorized user to the root endpoint of your application and end with the completely rendered web-page that is returned to the user.

Make sure that your illustration is complete. That is, likely for many of you there will be different kinds of "calls" and responses. Some HTTP calls and responses, some calls and responses in C# and likely some more. (Note the previous sentence is vague on purpose. I want that you create a complete illustration.)
# Process

## Build, test, release, and deployment
Illustrate with a UML activity diagram how your Chirp! applications are build, tested, released, and deployed. That is, illustrate the flow of activities in your respective GitHub Actions workflows.
Describe the illustration briefly, i.e., how your application is built, tested, released, and deployed.
![Diagram of our deployment workflow](images/deployazure.svg)
Note: I have taken the liberty of making the lines from negative conditions red, to make the diagram more readable considering that there are a considerable amount of points of failure. In addition, some repetitive steps(like checking out the repository, cleaning up ect) have been omitted

Note that the diagram doesn't include the release process, since it wasn't a part of the process at the time of writing(Essentially we let Chirp be a web-only application). 

In our case a successful deployment requires 4 parallel processes to all succeed, where 3 of these are tests. In the diagram these sub-processes are marked within the larger *Deploy to Azure* process. They are, from left to right 
- Test Migration: 
  - This workflow ensures that any possible migrations the deployment might want to apply to the production database won't break anything, by mimicking the migration 1:1. Obviously this would be unfeasible in a larger application with a db of many TBs, here you would instead create a database with an identical schema and seed it with some small, representative sample of the real database.
  - This is important as we have one *persistant* database across Chirp's entire lifetime, instead of just pushing a *chirp.db* file with every deployment(thus resetting the database on every deployment, which seemed undesirable) 
- Unit/Integration tests: 
  - By far the simplest of the test workflows. Here we just figure out what test projects exit(omitting UI-tests since they require a lot of additional set-up) and then run those test projects in parallel. 
- UI Tests: 
  - The bulk of this workflow is in setting up for the UI tests. The way they are implemented, the test runner expects an up to date Chirp.Web binary in it's own bin/ folder. Whatever binary is there is what will be tested, so for accurate tests we have to make sure we export the newest version of Chirp.Web. 
  - In addition to that, Playwright just has a lot of large dependencies(powershell and several browsers(who we cache since they take up ~500MB)) that aren't installed by default on the github actions machines. 
- Deployment Setup 
  - Here we build the Chirp.Web project binaries that we want to deploy as well as the bundled migration we (might) want to apply to the production database. Note that we always push a bundle, even if there is no new migrations to apply. In that case nothing happens when you try to apply the migration on the server. Naturally we can't deploy if we fail to generate either of these artifacts, but this step should usually succeed. 

If a single step fails, the entire workflow fails and nothing will be deployed. 

### Issues/Points of improvement 
As one can see, there is a few instances of redundancy in the workflow. The worst offender is probably the fact that we generate the exact same migration bundle and Chirp.Web binaries twice; once for testing and once for the actual deployment. Redudancy in, say, setup dotnet is immaterial considering how little time that action usually takes to execute. 

The reason for this redundancy is that the three test-workflows of the deployment workflows are, in Github actions, an entirely different workflow which is called on every push to every branch. The deployment workflow just calls this workflow and has the actual deployment action depend on it's success. 

This then also means that every push on main has the same tests run on it twice; the test workflow is triggered once directly by the push and once by the deployment workflow. 

The double generation of the migration bundle and Chirp.Web binaries could be solved by having the binaries as an output of the Test workflow and input of the Deployment workflow, and the double running of the tests could be solved by either having the test workflow explicitly only trigger on *non-main* branches or by having the deployment workflow query if a successful test run on the same commit exists. 

## Team work
Below is an image of our project board on GitHub right before hand-in. As seen in the picture there are unfinished issues. The unfinished issues are from the wild style week that aren't implemented due to focusing on higher priority issues based on the project requirements or time constraints. On the project board, it is visible that each issue are assigned to one or more team members. 
![Illustration of issue activities](images/Project_board.png)

When issues were created a person/persons were assigned for the responsibility of the new feature. The responsible person creates a branch and starts working on the issue. Once an issue is done the assigned person submits a pull request and the PR is tested and reviewed by another team member that hasn't worked on the specific issue. This part of the timeline is iterative, meaning that if the PR is not approved or fails tests, the assigned person will continuously improve it until it is approved, and the branch is successfully merged into the main branch. After this step will the issue be moved to ‘Done’ on our project board.

![Illustration of issue activities](images/Issue_Diagram.svg)

## How to make _Chirp!_ work locally
How to Git Clone and Run the Program: 

1. Open a new terminal window at the preferred directory and run the following command: 
`git clone https://github.com/ITU-BDSA2024-GROUP26/Chirp.git`

2. Navigate to the Chirp directory, once the cloning has finished: 
`cd Chirp`

3. Navigate to the src folder, where the source code lies: 
`cd src`

4. Navigate to the Web folder, containing the razor pages and program: 
`cd Web`

5. Once in the web directory, run the program: 

    1. To run the program on Windows, write following command : 
    `& { $env:ASPNETCORE_ENVIRONMENT = "Development"; dotnet run }`

    2. To run the program on MacOS and Linux, write following command: 
    `ASPNETCORE_ENVIRONMENT=Development dotnet run`
     
Once the build has finished, this line should be visible containing a link to the localhost:  
`Now listening on: http://localhost:5273`

Clicking on the link will direct to the locally run Chirp! application. 

## How to run test suite locally
List all necessary steps that Adrian or Helge have to perform to execute your test suites. Here, you can assume that we already cloned your repository in the step above.

Briefly describe what kinds of tests you have in your test suites and what they are testing.
# Ethics

## License
State which software license you chose for your application.

We have chosen the standard MIT License for its simplicity and widespread use. The license is commonly used with the .NET, which is the main platform we are working with.

## LLMs, ChatGPT, CoPilot, and others
During the development of our project, we used the following LLMs: ChatGPT GitHub and Copilot. ChatGPT was primarily used when we needed clarification, a better overview, boilerplate code or help understanding specific errors and bugs that we couldn't resolve within the group. It assisted us through the project development with issues and questions, where the textbook material wasn't enough to guide us. On the other hand, Copilot was more code-specific, directly assisting us in writing and completing code.

Both of the LLMs improved our productivity by saving time on repetitive tasks, such as generating boilerplate code or rewriting previously written code by the group.  The biggest disadvantage we noticed when using ChatGPT, was that we had to be careful with our prompts and know exaclty what we wanted to ask, to receive relevant and helpful responses. At times, especially for obscure code-related issues, ChatGPT's answers weren't useful, requiring us to either rephrase our questions, or simply not rely on ChatGPT for that issue.

