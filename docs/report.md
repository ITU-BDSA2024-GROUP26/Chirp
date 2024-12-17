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
## Architecture of deployed application
Illustrate the architecture of your deployed application. Remember, you developed a client-server application. Illustrate the server component and to where it is deployed, illustrate a client component, and show how these communicate with each other.
## User activities
Illustrate typical scenarios of a user journey through your Chirp! application. That is, start illustrating the first page that is presented to a non-authorized user, illustrate what a non-authorized user can do with your Chirp! application, and finally illustrate what a user can do after authentication.

Make sure that the illustrations are in line with the actual behavior of your application.
## Sequence of functionality/calls trough _Chirp!_
With a UML sequence diagram, illustrate the flow of messages and data through your Chirp! application. Start with an HTTP request that is send by an unauthorized user to the root endpoint of your application and end with the completely rendered web-page that is returned to the user.

Make sure that your illustration is complete. That is, likely for many of you there will be different kinds of "calls" and responses. Some HTTP calls and responses, some calls and responses in C# and likely some more. (Note the previous sentence is vague on purpose. I want that you create a complete illustration.)
# Process

## Build, test, release, and deployment
Illustrate with a UML activity diagram how your Chirp! applications are build, tested, released, and deployed. That is, illustrate the flow of activities in your respective GitHub Actions workflows.

Describe the illustration briefly, i.e., how your application is built, tested, released, and deployed.
## Team work
Show a screenshot of your project board right before hand-in. Briefly describe which tasks are still unresolved, i.e., which features are missing from your applications or which functionality is incomplete.

Briefly describe and illustrate the flow of activities that happen from the new creation of an issue (task description), over development, etc. until a feature is finally merged into the main branch of your repository.

## How to make _Chirp!_ work locally
There has to be some documentation on how to come from cloning your project to a running system. That is, Adrian or Helge have to know precisely what to do in which order. Likely, it is best to describe how we clone your project, which commands we have to execute, and what we are supposed to see then

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

    1. To run the program on Windows, write following command: 
    `$env:ASPNETCORE_ENVIRONMENT= "Development"`

    2. To run the program on MacOS, write following command: 
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
State which LLM(s) were used during development of your project. In case you were not using any, just state so. In case you were using an LLM to support your development, briefly describe when and how it was applied. Reflect in writing to which degree the responses of the LLM were helpful. Discuss briefly if application of LLMs sped up your development or if the contrary was the case.

During the development of our project, we used the following LLMs: ChatGPT GitHub and Copilot. ChatGPT was primarily used when we needed clarification, a better overview, or help understanding specific errors and bugs that we couldn't resolve within the group. It assisted us through the project development with issues and questions where the textbook material wasn't enough to guide us. On the other hand, Copilot was more code-specific, directly assisting us in writing and completing code.

Both of the LLMs improved our productivity by saving time on repetitive tasks, such as generating boilerplate code or rewriting previously written code by the group. The biggest disadvantage we noticed when using ChatGPT, was that we had to be careful with our prompts and know exaclty what we wanted to ask, to receive relevant and helpful responses. At times, especially for obscure code-related issues, ChatGPT's answers weren't useful, requiring us to either rephrase our questions, or simply not rely on ChatGPT for that issue.

