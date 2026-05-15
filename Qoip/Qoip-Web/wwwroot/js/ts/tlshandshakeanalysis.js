var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import axios from '../lib/axios/axios.min.js';
import { createApp, defineComponent } from '../lib/vue/vue.esm-browser.js';
const axiosClient = axios;
const app = createApp(defineComponent({
    data() {
        return {
            host: '',
            port: 443,
            timeout: 5000,
            response: null,
            loading: false,
            error: null,
            copied: false
        };
    },
    methods: {
        performRequest() {
            return __awaiter(this, void 0, void 0, function* () {
                this.loading = true;
                this.error = null;
                this.response = null;
                try {
                    const response = yield axiosClient.get('/api/NetworkSecurity/tls-handshake', {
                        params: {
                            host: this.host,
                            port: this.port,
                            timeout: this.timeout
                        }
                    });
                    this.response = response.data;
                    this.copied = false;
                }
                catch (error) {
                    console.error('Error performing TLS handshake analysis:', error);
                    this.error = 'Error performing TLS handshake analysis. Please try again.';
                }
                finally {
                    this.loading = false;
                }
            });
        },
        copyResponse() {
            return __awaiter(this, void 0, void 0, function* () {
                if (!this.response) {
                    return;
                }
                yield navigator.clipboard.writeText(this.formatJson(this.response));
                this.copied = true;
            });
        },
        formatJson(value) {
            return JSON.stringify(value, null, 2);
        },
        clearForm() {
            this.host = '';
            this.port = 443;
            this.timeout = 5000;
            this.response = null;
            this.error = null;
            this.copied = false;
        }
    }
}));
app.mount('#app');
