Feature: Registering a client in service discovery application

    As a user I want to register my client in service discovery application
    so that other applications will know about my application.

Scenario: Client should be successfully registered when I specify correct options
Given service discovery application is up and running
When user register application with id "fake-app-id" and address "10.1.1.2"
Then discovery service application returns status code Ok
    And application with id "fake-app-id" and address "10.1.1.2" is successfully registered
