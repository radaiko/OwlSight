import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'OwlSight',
  description: 'On-premise AI-powered code review tool for CI/CD pipelines',

  base: '/OwlSight/',
  appearance: 'dark',
  cleanUrls: true,

  head: [
    ['link', { rel: 'preconnect', href: 'https://fonts.googleapis.com' }],
    ['link', { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' }],
    ['link', { href: 'https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&display=swap', rel: 'stylesheet' }],
  ],

  themeConfig: {
    search: {
      provider: 'local',
    },

    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'CLI Reference', link: '/cli/review' },
      { text: 'Portfolio', link: 'https://radaiko.github.io' },
    ],

    sidebar: [
      {
        text: 'Guide',
        items: [
          { text: 'Home', link: '/' },
          { text: 'Getting Started', link: '/guide/getting-started' },
          { text: 'Configuration', link: '/guide/configuration' },
          { text: 'Custom Rules', link: '/guide/custom-rules' },
          { text: 'CI/CD Integration', link: '/guide/ci-cd' },
          { text: 'Docker', link: '/guide/docker' },
        ],
      },
      {
        text: 'CLI Reference',
        items: [
          { text: 'review', link: '/cli/review' },
          { text: 'init', link: '/cli/init' },
        ],
      },
      {
        text: 'Architecture',
        items: [
          { text: 'Agentic Loop', link: '/guide/agentic-loop' },
          { text: 'LLM Tools', link: '/guide/llm-tools' },
        ],
      },
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/radaiko/OwlSight' },
    ],
  },
})
