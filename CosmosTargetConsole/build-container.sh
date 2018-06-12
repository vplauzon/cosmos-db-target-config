#	Build docker container
sudo docker build -t vplauzon/cosmos-db-target-config .

#	Publish image
sudo docker push vplauzon/cosmos-db-target-config

#	Test image
#sudo docker run -it vplauzon/cosmos-db-target-config bash
