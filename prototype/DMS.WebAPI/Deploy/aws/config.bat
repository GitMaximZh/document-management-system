aws configure
[settings: user id, key...  !!!! never use global key and full access rights account]

aws ecr get-login --no-include-email --region us-east-1
[to get token]

#one repository per image
docker login -u AWS -p [tocken] https://[id].dkr.ecr.us-east-1.amazonaws.com


# tag image according to repository name
docker tag 9adab3126e03 263103933285.dkr.ecr.us-east-1.amazonaws.com/mzh_on_aws

docker push 263103933285.dkr.ecr.us-east-1.amazonaws.com/mzh_on_aws


#send taks definition
#use link for linking
aws ecs register-task-definition --cli-input-json file://dms-web-api-aws.json

#run task
aws ecs run-task --task-definition web-application