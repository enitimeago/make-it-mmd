import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
    site: 'https://make-it-mmd.tmgo.dev',
    redirects: {
        '/': '/ja/'
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
                    translations: {
                        'ja': 'ガイド',
                    },
                    items: [
                        {
                            label: 'Getting Started',
                            slug: 'guides/getting-started',
                            translations: {
                                'ja': 'はじめに',
                            },
                        },
                        {
                            label: 'Basic Usage',
                            slug: 'guides/basic-usage',
                            translations: {
                                'ja': '基本な使い方',
                            },
                        },
                    ],
                },
                {
                    label: 'Reference',
                    translations: {
                        'ja': 'ドキュメント',
                    },
                    autogenerate: { directory: 'reference' },
                },
            ],
        }),
    ],
});
