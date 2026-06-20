# syntax=docker/dockerfile:1

# ---- build stage ----
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . ./
RUN npm run build -- --configuration production

# ---- runtime stage ----
FROM nginx:1.27-alpine AS runtime

# Default upstreams (overridable per environment via env vars). The nginx image runs
# envsubst over files in /etc/nginx/templates at container start.
ENV CATALOG_HOST=catalog-service:8080 \
    SALES_HOST=sales-service:8080

COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80
