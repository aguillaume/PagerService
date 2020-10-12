# PagerService
Pager Service 

## How to run tests for solution
- clone the git repo locally
- Using Visual Studio open the AlertNotificationSystem.sln file
- Navigate to Buid > Build Solution
  - This will build the solution and allow the unit tests to tbe discoverables 
- Navigate to Test > Run > All Tests
  - This will run all unit tests in the solution which will show in a Test Explorer window

## Questions / Assumptions
  
  In user Story 5, "when the Pager receives a Healthy event related to this Monitored Service"
  
    I assumed the source of the event would be the console becasue it was not clear what the source was. Also, I did not make this an actual event as the rest of the system did not seem to relay on events but method exchanges. 
  
  In User Story 5 "and later receives the Acknowledgement Timeout, then the Pager doesn’t notify any Target"
   
    This does not seem like a very good criteria. the delay could have just been triggered, which means that for an other 15min the service will appear unhealthy until the Acknowledgement Timeout comes back.

  The notification targets (email & SMS) with the corresponding adapters could be implemented with generics for a more Open-Close apprach.

## Concurrency Issues & Detabase Guarantees
- I am expecting the database for the Pager Service to act as the stete management for each monitored service. 
- I expect the database to have a strong consistency with the service. this will allow a correct view of the service health when required. 