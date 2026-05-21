# Serve static frontend assets via Nginx.
FROM nginx:alpine

# Configure Nginx to listen on port 3000 for ECS target group compatibility.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy the built/static entry page and all frontend assets into the Nginx web root.
COPY index.html /usr/share/nginx/html/index.html
COPY favicon.png ADrippingMarker-Ddw1.ttf 456dicebgrd.png 456ranklogo.png /usr/share/nginx/html/
COPY images /usr/share/nginx/html/images/

EXPOSE 3000