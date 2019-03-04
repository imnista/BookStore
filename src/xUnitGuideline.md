# Welcome to xUnit hello world

## Reference
- http://svusindgithub/NGSPrep/Documents/blob/586f7787aecb78fc6a1cf6e7d514fbb2d202f8fb/testing-tool-guides/c-sharp-testing.org
  
Note:
 for the test class naming conventions, please use XxxTests, here Xxx is the test target.

## Recommendations
### Naming Conventions

Test Target Name | Type | Unit test naming convention | Remark
--|--|-- | -- 
AbcPrj | project | AbcPrj.Tests | 
Xyz | class | XyzTests | 
IsValid | method| IsValid_GivenXxx_Expected | 

### Code Structure
* Keep the same folder hierachy
* Namespace: add Tests.Prefix
