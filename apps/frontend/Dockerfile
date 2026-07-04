# Stage 1: Dependencies
FROM node:20-alpine AS deps
WORKDIR /app

# Copy package files for all workspaces
COPY package.json package-lock.json* ./
COPY apps/web/package.json ./apps/web/
COPY packages/ui/package.json ./packages/ui/
COPY packages/typescript-config/package.json ./packages/typescript-config/
COPY packages/eslint-config/package.json ./packages/eslint-config/

RUN npm ci

# Stage 2: Build
FROM node:20-alpine AS build
WORKDIR /app

COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Set backend URL for Next.js rewrites at build time
ARG BACKEND_API_URL=http://backend:5101
ENV BACKEND_API_URL=$BACKEND_API_URL

RUN npm run build

# Stage 3: Runtime
FROM node:20-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000

# Copy standalone output
COPY --from=build /app/apps/web/.next/standalone ./
COPY --from=build /app/apps/web/.next/static ./apps/web/.next/static
COPY --from=build /app/apps/web/public ./apps/web/public

EXPOSE 3000

CMD ["node", "apps/web/server.js"]