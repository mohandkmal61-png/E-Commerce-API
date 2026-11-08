# E-Commerce Storefront

Responsive React, TypeScript, and Vite storefront connected to the E-Commerce API.

## Requirements

- Node.js 20.19+ or 22.12+
- npm

## Run locally

From this directory:

```sh
npm install
Copy-Item .env.example .env.local
npm run dev
```

Open the URL printed by Vite (normally `http://localhost:5173`).

The development server proxies `/api` requests to `http://localhost:5187`.
Set `VITE_API_BASE_URL` in `.env.local` if the API is hosted at another origin.
The storefront reads products, basket data, authentication, delivery methods,
and orders from the backend API.

Guest basket credentials are generated in the browser and persisted in
`localStorage`; they are sent to the API using `X-Basket-Guest-Secret`. After
sign-in, the storefront merges that guest basket into the authenticated user's
basket and retains the guest credentials if the merge fails.

Checkout is deliberately non-charging: it requests a Stripe PaymentIntent and
creates a pending order without collecting card details or confirming payment.
To exercise it, configure the backend with a Stripe **test** secret key and
seed at least one delivery method and product. Never configure a live key for
development checkout.

## Available scripts

- `npm run dev` — start the Vite development server.
- `npm run build` — type-check and create a production build.
- `npm run preview` — preview the production build locally.
