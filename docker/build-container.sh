#	Remove temp directory
rm temp -r

#	Create temp directory
mkdir temp

#	Build console app & drop them in temp folder
dotnet build ../CosmosTargetConsole/ -c release -o ../docker/temp

#	Build docker container
sudo docker build -t vplauzon/cosmos-db-target-config .

#	Publish image
sudo docker push vplauzon/cosmos-db-target-config

#	Test image
#sudo docker run -it vplauzon/cosmos-db-target-config
