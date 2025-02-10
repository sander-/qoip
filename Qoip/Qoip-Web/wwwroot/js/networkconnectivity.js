var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
const app = createApp(defineComponent({
    data() {
        return {
            domain: '',
            dnsResponse: null
        };
    },
    methods: {
        performDnsRequest() {
            return __awaiter(this, void 0, void 0, function* () {
                try {
                    const response = yield axios.get(`/api/NetworkConnectivity/dns`, {
                        params: {
                            domain: this.domain
                        }
                    });
                    this.dnsResponse = response.data;
                }
                catch (error) {
                    console.error('Error performing DNS request:', error);
                    this.dnsResponse = { status: 'error', data: 'Error performing DNS request. Please try again.' };
                }
            });
        }
    }
}));
app.mount('#app');
