var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
const app = createApp(defineComponent({
    data() {
        return {
            clientIpInfo: null,
            whoisInfo: {},
            browserAgentInfo: navigator.userAgent,
            platformInfo: navigator.platform,
            languageInfo: navigator.language,
            loading: false,
            whoisLoading: {},
            error: null,
            whoisError: {}
        };
    },
    methods: {
        fetchClientIpInfo() {
            return __awaiter(this, void 0, void 0, function* () {
                this.loading = true;
                this.error = null;
                this.clientIpInfo = null;
                try {
                    const response = yield axios.get('/api/networkconnectivity/client-ip');
                    this.clientIpInfo = response.data;
                }
                catch (error) {
                    console.error('Error fetching client IP information:', error);
                    this.error = 'Failed to load client IP information.';
                }
                finally {
                    this.loading = false;
                }
            });
        },
        fetchWhoisInfo(ipAddress) {
            return __awaiter(this, void 0, void 0, function* () {
                this.whoisLoading[ipAddress] = true;
                this.whoisError[ipAddress] = null;
                this.whoisInfo[ipAddress] = null;
                try {
                    const response = yield axios.get(`/api/networkconnectivity/whois?ipAddress=${ipAddress}`);
                    this.whoisInfo[ipAddress] = response.data;
                }
                catch (error) {
                    console.error(`Error fetching WHOIS information for ${ipAddress}:`, error);
                    this.whoisError[ipAddress] = 'Failed to load WHOIS information.';
                }
                finally {
                    this.whoisLoading[ipAddress] = false;
                }
            });
        }
    }
}));
app.mount('#this-is-you-app');
