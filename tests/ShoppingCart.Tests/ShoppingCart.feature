Feature: Add and remove items from shopping cart

Scenario 1: Adding items
        Given an empty shopping cart
        When I add 3 bacon
        And I add 2 egg
        Then there should be 3 bacon in the shopping cart
        And there should be 2 egg in the shopping cart

Scenario 2: Removing items
        Given an empty shopping cart
        When I add a bacon
        And I add 5 egg
        And I remove a bacon
        And I remove an egg
        Then there should be 4 egg in the shopping cart
        And there should be 0 bacon in the shopping cart

Scenario 3: Removing more items than there are in the shopping cart
        Given an empty shopping cart
        When I add an egg
        And I remove 3 egg
        Then there should be 0 egg in the shopping cart

Scenario 4: Removing items from empty shopping cart
        Given an empty shopping cart
        When I remove a bacon
        Then there should be 0 bacon in the shopping cart

Scenario 5: Empty shopping cart contains nothing
        Given an empty shopping cart
        Then there should be 0 bacon in the shopping cart
        And there should be 0 egg in the shopping cart
