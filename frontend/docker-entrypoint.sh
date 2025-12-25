#!/bin/sh

# Generate config.js from environment variables
cat <<EOF > /usr/share/nginx/html/config.js
window.config = {
  VITE_API_BASE_URL: "${VITE_API_BASE_URL:-http://localhost:8080}"
};
EOF

# Start Nginx
exec "$@"
