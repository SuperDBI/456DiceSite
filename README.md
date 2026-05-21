# 456Dice Site

## Build and Deployment Notes

- The frontend Docker image is built and deployed through GitHub Actions.
- Do not attempt to build the frontend Docker image locally unless you have Docker installed and intentionally want to test it.
- The primary build workflow runs in GitHub and pushes the image to ECR.

## Important

- If you see a local Docker build failure on your machine, that does not mean the GitHub Actions build is failing.
- Verify the GitHub Actions workflow logs for the actual frontend image build status.
