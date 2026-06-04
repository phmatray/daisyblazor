// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
  site: 'https://phmatray.github.io',
  base: '/daisyblazor',
  integrations: [
    starlight({
      title: 'DaisyBlazor',
      description:
        'A Tailwind CSS v4 + daisyUI v5 component library for Blazor.',
      social: [
        {
          icon: 'github',
          label: 'GitHub',
          href: 'https://github.com/phmatray/daisyblazor',
        },
      ],
      editLink: {
        baseUrl:
          'https://github.com/phmatray/daisyblazor/edit/main/website/',
      },
      sidebar: [
        {
          label: 'Start here',
          items: [
            { label: 'Getting started', slug: 'getting-started' },
            { label: 'Theming', slug: 'theming' },
          ],
        },
        {
          label: 'Styling',
          items: [{ label: 'CSS preset', slug: 'css-preset' }],
        },
        {
          label: 'Components',
          items: [{ label: 'Component reference', slug: 'components' }],
        },
        {
          label: 'Charts',
          items: [{ label: 'Charts', slug: 'charts' }],
        },
        {
          label: 'Migrating',
          items: [{ label: 'From MudBlazor', slug: 'migration' }],
        },
        {
          label: 'API reference',
          items: [{ autogenerate: { directory: 'api' } }],
        },
      ],
    }),
  ],
});
