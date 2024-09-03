import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
    site: 'https://enitimeago.github.io',
    base: 'make-it-mmd',
    redirects: {
        '/': '/make-it-mmd/ja/'
    },
    integrations: [
        starlight({
            title: 'Make It MMD',
            customCss: [
                './src/styles/custom.scss',
                '@fontsource-variable/public-sans/wght.css',
            ],
            credits: true,
            components: {
                Footer: './src/components/Footer.astro',
            },
            defaultLocale: 'en',
            locales: {
                en: {
                    label: 'English',
                },
                ja: {
                    label: '日本語',
                },
            },
            social: {
                github: 'https://github.com/enitimeago/make-it-mmd',
            },
            sidebar: [
                {
                    label: 'Guides',
                    items: [
                        {
                            label: 'Getting Started',
                            slug: 'guides/getting-started',
                            translations: {
                                'ja': 'セットアップガイド',
                            },
                        },
                    ],
                },
                {
                    label: 'Reference',
                    autogenerate: { directory: 'reference' },
                },
            ],
        }),
    ],
});
