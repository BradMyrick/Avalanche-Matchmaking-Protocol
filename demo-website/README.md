# AMP Demo Website

This directory contains the one-page HTML "investable" landing page for the AMP Protocol MVP. It visualizes the core value proposition, architecture, and developer workflow without requiring a complex web server framework.

## Viewing Locally

Since this is purely static HTML/CSS/JS, simply open the file in your browser:

```bash
# On Mac:
open index.html

# On Linux:
xdg-open index.html
```

## Features
- **Visual Integration Flow**: A Javascript-simulated terminal console that steps through the actual Cap'n Proto capability exchange and Anvil verification steps without needing the backend running.
- **Glassmorphism UI**: Uses modern CSS backdrop filters to mimic a premium web application.
- **No Dependencies**: Zero npm install required.

## GitHub Pages Hosting

This site is optimized to be served directly from GitHub Pages.

To deploy:
1. Go to your repository **Settings** on GitHub.
2. Select **Pages** on the left menu.
3. Choose the branch `main` (or whichever branch your MVP is on).
4. Select the `/demo-website` directory if your repository structure supports it, or change the root deployment to point to `demo-website/index.html`.
5. Save. Your site will soon be live at your GitHub Pages URL!
