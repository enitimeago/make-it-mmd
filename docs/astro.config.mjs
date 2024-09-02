import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
    redirects: {
        '/': '/ja/'
    },
	integrations: [
		starlight({
			title: 'Make It MMD',
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
