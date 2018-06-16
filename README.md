# Cosmos DB Target Config

Configuration Mangement solution for  Cosmos DB account.

See **TODO**[this article]() explaining the objective of the solution and how to use it.

Example of usage can be found in this [ARM Template](https://github.com/vplauzon/cosmos-db-target-config/blob/master/Deployment/azuredeploy.json) which can be deployed here:

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fvplauzon%2Fcosmos-db-target-config%2Fmaster%2FDeployment%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>
<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fvplauzon%2Fcosmos-db-target-config%2Fmaster%2FDeployment%2Fazuredeploy.json" target="_blank">
    <img src="http://armviz.io/visualizebutton.png"/>
</a>

It is recommended to use this solution packaged in a container.  Find the container image on [Docker Hub](https://hub.docker.com/r/vplauzon/cosmos-db-target-config/).

This solution currently supports:

* Databases
* Collections
* Stored Procedures

On "roadmap", or natural extension, would be:

* User Defined Function
* Index Policy