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
            url: '',
            timeout: 5000,
            response: null,
            loading: false,
            error: null
        };
    },
    methods: {
        performRequest() {
            return __awaiter(this, void 0, void 0, function* () {
                this.loading = true;
                this.error = null;
                this.response = null;
                try {
                    const response = yield axios.get('/api/NetworkSecurity/security-headers', {
                        params: {
                            url: this.url,
                            timeout: this.timeout
                        }
                    });
                    this.response = response.data;
                }
                catch (error) {
                    console.error('Error performing security header analysis:', error);
                    this.error = 'Error performing security header analysis. Please try again.';
                }
                finally {
                    this.loading = false;
                }
            });
        },
        clearForm() {
            this.url = '';
            this.timeout = 5000;
            this.response = null;
            this.error = null;
        }
    }
}));
app.mount('#app');
