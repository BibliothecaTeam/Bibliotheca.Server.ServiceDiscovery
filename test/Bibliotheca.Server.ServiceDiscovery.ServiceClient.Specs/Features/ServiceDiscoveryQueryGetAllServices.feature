Feature: Getting all services from service discovery application

    As a user I want to get information about all registered application
    so that I can use that information to prepare communication with them.

Scenario: Service discovery application should return information about registered services when user get all services
Given service discovery application is up and running
    And application "fake-getall-id1" is registered
    And application "fake-getall-id2" is registered
When user get information about registered services
Then application with id "fake-getall-id1" exists on list
    And application with id "fake-getall-id2" exists on list
