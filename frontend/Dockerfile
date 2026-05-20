# Serve static frontend assets via Nginx.
FROM nginx:alpine

# Configure Nginx to listen on port 3000 for ECS target group compatibility.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy the built/static entry page into Nginx default web root.
COPY index.html /usr/share/nginx/html/index.html

EXPOSE 3000