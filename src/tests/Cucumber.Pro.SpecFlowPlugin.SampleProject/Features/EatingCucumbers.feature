@smoke
Feature: Eating cucumbers

  @sample
  Scenario: Many cucumbers
    Given I have already eaten 99 cucumbers
    When I eat 47 cucumbers
    Then I should have 52 cucumbers in my belly

  Scenario: Few cucumbers
    Given I have already eaten 5 cucumbers
    When I eat 2 cucumbers
    Then I should have 7 cucumbers in my belly

  Scenario Outline: Few cucumbers outline
    Given I have already eaten 5 cucumbers
    When I eat 2 cucumbers
    Then I should have 7 cucumbers in my belly
  Examples:
    | a | b | c |
    | 5 | 2 | 7 |
    | 6 | 2 | 8 |


Scenario: Undefined cucumbers
    Given I have already eaten 5 cucumbers
    When there is an undefined step

Scenario: Pending cucumbers
    Given I have already eaten 5 cucumbers
    When there is a pending step

Scenario: Skipped cucumbers
    Given I have already eaten 5 cucumbers
    When there is an error
    Then the last step is skipped

@beforescenario_error
Scenario: Scenario with a BeforeScenario error
    Given I have already eaten 5 cucumbers

@afterscenario_error
Scenario: Scenario with a AfterScenario error
    Given I have already eaten 5 cucumbers
