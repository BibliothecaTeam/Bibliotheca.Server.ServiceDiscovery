Feature: Getting service by id have to return correct information about services

    As a user I want to get information about specific services 
    so that I can get information about that services.

Scenario: Service discovery application should return information about registered services when user get service by tag
Given service discovery application is up and running
    And application "fake-getbytag-id1" is registered with tag "tag1"
    And application "fake-getbytag-id1" is registered with tag "tag1"
    And application "fake-getbytag-id2" is registered with tag "tag2"
    And application "fake-getbytag-id3" is registered with tag "tag3"
When user get information about services with tag "tag1"
Then two applications with prefix id "fake-getbytag-id1" should be returned
