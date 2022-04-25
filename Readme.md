# Helpful commands to use

## Platform Service

```
dotnet run

docker build -t grenkin/platformservice .

docker push grenkin/platformservice
```

## K8S

```
kubectl apply -f platforms-depl.yaml

kubectl apply -f platforms-np-srv.yaml

kubectl apply -f commands-depl.yaml

kubectl get deployments
kubectl get pods
kubectl get services

kubectl rollout restart deployment platforms-depl
```

## Commands Service

```
dotnet run

docker build -t grenkin/commandservice .

docker push grenkin/commandservice

docker run -p 8080:80 grenkin/commandservice
```
