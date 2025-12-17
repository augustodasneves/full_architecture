FROM node:20-alpine

WORKDIR /app

# Install git (required by Baileys)
RUN apk add --no-cache git

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install --production

# Copy application code
COPY . .

# Create directory for auth data
RUN mkdir -p /app/auth_info

# Expose port
EXPOSE 3000

# Start the service
CMD ["npm", "start"]
