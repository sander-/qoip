import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import { AxiosResponse } from 'axios';

interface ClientIpResponse {
    clientIpAddress: string;
    clientIpCanonical: string | null;
    iPv4Address: string | null;
    iPv4Canonical: string | null;
    iPv6Address: string | null;
    iPv6Canonical: string | null;
    proxyAddresses: string[];
    realIpAddress: string;
}

interface WhoisResponse {
    whoisData: string;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                clientIpInfo: null as ClientIpResponse | null,
                whoisInfo: {} as Record<string, WhoisResponse | null>,
                browserAgentInfo: navigator.userAgent,
                platformInfo: navigator.platform,
                languageInfo: navigator.language,
                loading: false,
                whoisLoading: {} as Record<string, boolean>,
                error: null as string | null,
                whoisError: {} as Record<string, string | null>
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
            },
            async fetchWhoisInfo(ipAddress: string) {
                this.whoisLoading[ipAddress] = true;
                this.whoisError[ipAddress] = null;
                this.whoisInfo[ipAddress] = null;
                try {
                    const response: AxiosResponse<WhoisResponse> = await axios.get(`/api/networkconnectivity/whois?ipAddress=${ipAddress}`);
                    this.whoisInfo[ipAddress] = response.data;
                    console.log(response);
                } catch (error) {
                    console.error(`Error fetching WHOIS information for ${ipAddress}:`, error);
                    this.whoisError[ipAddress] = 'Failed to load WHOIS information.';
                } finally {
                    this.whoisLoading[ipAddress] = false;
                }
            }
        }
    })
);

app.mount('#this-is-you-app');
