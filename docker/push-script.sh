#!/bin/bash

appVersion="8.3.0"
acrBase="logicalis.azurecr.io/vm-exporter"

docker compose -f docker-compose.build.yml --build-arg APP_VERSION=${APP_VERSION} build

# Tagging Docker images for AMD64 architecture
docker tag vm-exporter/vsphere-vm-exporter-backend:${appVersion}-amd64 ${acrBase}/vsphere-vm-exporter-backend:latest-amd64
docker tag vm-exporter/vsphere-vm-exporter-backend:${appVersion}-amd64 ${acrBase}/vsphere-vm-exporter-backend:${appVersion}-amd64
docker tag vm-exporter/vsphere-vm-exporter-frontend:${appVersion}-amd64 ${acrBase}/vsphere-vm-exporter-frontend:latest-amd64
docker tag vm-exporter/vsphere-vm-exporter-frontend:${appVersion}-amd64 ${acrBase}/vsphere-vm-exporter-frontend:${appVersion}-amd64

# Tagging Docker images for ARM64 architecture
docker tag vm-exporter/vsphere-vm-exporter-backend:${appVersion}-arm64 ${acrBase}/vsphere-vm-exporter-backend:latest-arm64
docker tag vm-exporter/vsphere-vm-exporter-backend:${appVersion}-arm64 ${acrBase}/vsphere-vm-exporter-backend:${appVersion}-arm64
docker tag vm-exporter/vsphere-vm-exporter-frontend:${appVersion}-arm64 ${acrBase}/vsphere-vm-exporter-frontend:latest-arm64
docker tag vm-exporter/vsphere-vm-exporter-frontend:${appVersion}-arm64 ${acrBase}/vsphere-vm-exporter-frontend:${appVersion}-arm64
clear


# pushing Docker images for ARM64 architecture
docker push ${acrBase}/vsphere-vm-exporter-backend:latest-arm64
docker push ${acrBase}/vsphere-vm-exporter-backend:${appVersion}-arm64
docker push ${acrBase}/vsphere-vm-exporter-frontend:latest-arm64
docker push ${acrBase}/vsphere-vm-exporter-frontend:${appVersion}-arm64

# pushing Docker images for AMD64 architecture
docker push ${acrBase}/vsphere-vm-exporter-backend:latest-amd64
docker push ${acrBase}/vsphere-vm-exporter-backend:${appVersion}-amd64
docker push ${acrBase}/vsphere-vm-exporter-frontend:latest-amd64
docker push ${acrBase}/vsphere-vm-exporter-frontend:${appVersion}-amd64

clear

echo "----------------------------------------"

echo "Docker images pushed successfully."

echo "----------------------------------------"
