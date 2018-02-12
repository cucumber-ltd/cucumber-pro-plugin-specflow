@smoke
Feature: EatingCucumbersWithBackground

Background:
    Given I have already eaten 5 cucumbers

Scenario: Few cucumbers with background
    When I eat 2 cucumbers
    Then I should have 7 cucumbers in my belly
