import type { NextConfig } from "next"

const nextConfig: NextConfig = {
  transpilePackages: ["@workspace/ui"],
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${process.env.BACKEND_API_URL}/api/:path*`,
      },
      {
        source: "/hubs/:path*",
        destination: `${process.env.BACKEND_API_URL}/hubs/:path*`,
      },
    ];
  },
}

export default nextConfig
