Feature: Getting service by id have to return correct information about services

    As a user I want to get information about specific services 
    so that I can get information about that services.

Scenario: Service discovery application should return information about registered services when get service by Id
Given service discovery application is up and running
    And application "fake-getbyids-id1" is registered
    And application "fake-getbyids-id2" is registered
    And application "fake-getbyids-id3" is registered
    And application "fake-getbyids-id4" is registered
When user get information about service "fake-getbyids-id1"
Then two applications with prefix id "fake-getbyids-id1" should be returned
