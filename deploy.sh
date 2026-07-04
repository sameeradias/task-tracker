#!/bin/bash
set -e

ECR_REGISTRY=$1
IMAGE_TAG=$2
AWS_REGION=$3

if [ -z "$ECR_REGISTRY" ] || [ -z "$IMAGE_TAG" ] || [ -z "$AWS_REGION" ]; then
    echo "Usage: deploy.sh <ECR_REGISTRY> <IMAGE_TAG> <AWS_REGION>"
    exit 1
fi

echo "=== Task Tracker Deployment ==="
echo "Registry: $ECR_REGISTRY"
echo "Tag: $IMAGE_TAG"
echo "Region: $AWS_REGION"
echo ""

# Login to ECR
echo "[1/5] Logging into ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_REGISTRY

# Pull latest images
echo "[2/5] Pulling images..."
docker pull $ECR_REGISTRY/task-tracker-backend:$IMAGE_TAG
docker pull $ECR_REGISTRY/task-tracker-frontend:$IMAGE_TAG

# Update .env with new image tags
echo "[3/5] Updating image references..."
sed -i "s|^BACKEND_IMAGE=.*|BACKEND_IMAGE=$ECR_REGISTRY/task-tracker-backend:$IMAGE_TAG|" .env
sed -i "s|^FRONTEND_IMAGE=.*|FRONTEND_IMAGE=$ECR_REGISTRY/task-tracker-frontend:$IMAGE_TAG|" .env

# Deploy with docker-compose
echo "[4/5] Deploying containers..."
docker compose down
docker compose up -d

# Cleanup old images
echo "[5/5] Cleaning up old images..."
docker image prune -f

echo ""
echo "=== Deployment Complete ==="
echo ""
docker compose ps