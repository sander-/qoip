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
            url: '',
            expirationWarningThresholdInDays: 0,
            validationResponse: null,
            loading: false,
            error: null
        };
    },
    methods: {
        performCertificateValidation() {
            return __awaiter(this, void 0, void 0, function* () {
                this.loading = true;
                this.error = null;
                this.validationResponse = null;
                try {
                    const response = yield axios.get(`/api/securityencryption/certificate`, {
                        params: {
                            url: this.url,
                            expirationWarningThresholdInDays: this.expirationWarningThresholdInDays
                        }
                    });
                    this.validationResponse = response.data;
                }
                catch (error) {
                    console.error('Error performing certificate validation:', error);
                    this.error = 'Error performing certificate validation. Please try again.';
                }
                finally {
                    this.loading = false;
                }
            });
        },
        clearForm() {
            this.url = '';
            this.expirationWarningThresholdInDays = 0;
            this.validationResponse = null;
            this.error = null;
        }
    }
}));
app.mount('#app');
