import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import { AxiosResponse } from 'axios';

interface ClientIpResponse {
    clientIpAddress: string;
    iPv4Address: string | null;
    iPv6Address: string | null;
    proxyAddresses: string[];
    realIpAddress: string;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                clientIpInfo: null as ClientIpResponse | null,
                browserAgentInfo: navigator.userAgent,
                platformInfo: navigator.platform,
                languageInfo: navigator.language,
                loading: false,
                error: null as string | null
            };
        },
        methods: {
            async fetchClientIpInfo() {
                this.loading = true;
                this.error = null;
                this.clientIpInfo = null;
                try {
                    const response: AxiosResponse<ClientIpResponse> = await axios.get('/api/networkconnectivity/client-ip');
                    this.clientIpInfo = response.data;
                } catch (error) {
                    console.error('Error fetching client IP information:', error);
                    this.error = 'Failed to load client IP information.';
                } finally {
                    this.loading = false;
                }
            }
        },
        mounted() {
            this.fetchClientIpInfo();
        }
    })
);

app.mount('#this-is-you-app');
