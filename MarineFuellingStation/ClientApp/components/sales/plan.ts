﻿import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from "axios";
import moment from "moment";

@Component
export default class PlanComponent extends Vue {
    radio2: string = '1';
    username: string;
    model: server.salesPlan;
    oildate: string;
    salesplans: server.salesPlan[];
    oilshow: boolean = false;
    oiloptions: ydui.actionSheetItem[];

    constructor() {
        super();

        (<any>this).$dialog.loading.open('很快加载好了');

        this.salesplans = (new Array()) as server.salesPlan[];
        this.oiloptions = (new Array()) as ydui.actionSheetItem[];

        this.model = (new Object()) as server.salesPlan;
        this.model.name = '';
        this.model.unit = '升';
        this.model.isInvoice = false;
        this.model.carNo = '';
        this.model.price = 0;
        this.model.count = 0;
        this.model.remainder = 0;
        this.model.oilDate = new Date();
        this.model.billingCompany = '';
        this.model.billingPrice = 0;
        this.model.billingCount = 0;
        this.model.productId = 0;
        this.model.oilName = '请选择油品';

        this.oildate = this.formatDate(this.model.oilDate);

        this.username = this.$store.state.username;
        this.getSalesPlanNo();
        this.getOilProducts();

        (<any>this).$dialog.loading.close();
    }

    mounted() {
        this.$emit('setTitle', this.username + ' 销售计划');
        //观察者模式
        this.$watch('radio2', (v, ov) => {
            switch (v) {
                case "1":
                    this.model.unit = '升';
                    this.model.salesPlanType = server.salesPlanType.水上;
                    break;
                case "2":
                    this.model.unit = '吨';
                    this.model.salesPlanType = server.salesPlanType.陆上;
                    break;
                case "3":
                    this.model.unit = '桶';
                    this.model.salesPlanType = server.salesPlanType.机油;
                    break;
            }
        });
        this.$watch('model.price', (v, ov) => {
            this.model.billingPrice = v;
        });
        this.$watch('model.count', (v, ov) => {
            this.model.billingCount = v;
        });
        this.$watch('oildate', (v, ov) => {
            this.model.oilDate = new Date(this.oildate);
        });
    };

    change(label: string, tabkey: string) {
        this.$emit('setTitle', this.username + ' ' + label);

        if (label == '单据记录')
            this.getSalesPlans();
    }

    buttonclick() {
        //信息验证
        if (this.model.carNo == '') {
            this.toastError('车牌不能为空');
            return;
        }
        if (this.model.count <= 0) {
            this.toastError('数量必须大于1');
            return;
        }
        if (this.model.productId == 0) {
            this.toastError('必须选择油品');
            return;
        }
        this.postSalesPlan(this.model);
    }

    formatDate(d: Date): string {
        return moment(d).format('YYYY-MM-DD');
    }

    getStateName(s: server.salesPlanState): string {
        switch (s) {
            case server.salesPlanState.未审批:
                return '未审批';
            case server.salesPlanState.已审批:
                return '已审批';
            case server.salesPlanState.已完成:
                return '已完成';
        }
    }

    toastError(msg: string) {
        (<any>this).$dialog.toast({
            mes: msg,
            timeout: 1500,
            icon: 'error'
        });
    }

    classState(s: server.salesPlanState): any {
        switch (s) {
            case server.salesPlanState.未审批:
                return { color_red: true }
            case server.salesPlanState.已审批:
                return { color_green: true }
            case server.salesPlanState.已完成:
                return { color_blue: true }
        }
    }

    getSalesPlanNo() {
        axios.get('/api/SalesPlan/SalesPlanNo').then((res) => {
            let jobj = res.data as server.resultJSON<string>;
            if (jobj.code == 0)
                this.model.name = jobj.data;
        });
    }

    getSalesPlans() {
        axios.get('/api/SalesPlan').then((res) => {
            let jobj = res.data as server.resultJSON<server.salesPlan[]>;
            if (jobj.code == 0)
                this.salesplans = jobj.data;
        });
    }

    getOilProducts() {
        axios.get('/api/Product/OilProducts').then((res) => {
            let jobj = res.data as server.resultJSON<server.product[]>;
            if (jobj.code == 0) {
                jobj.data.forEach((o, i) => {
                    this.oiloptions.push({
                        label: o.name,
                        method: () => {
                            this.model.oilName = o.name;
                            if (o.lastPrice > 0)
                                this.model.price = o.lastPrice;
                            else
                                this.model.price = o.minPrice;
                        }
                    });
                });
            }
        });
    }

    postSalesPlan(model: server.salesPlan) {
        axios.post('/api/SalesPlan', model).then((res) => {
            let jobj = res.data as server.resultJSON<string>;
            if (jobj.code == 0)
                (<any>this).$dialog.toast({
                    mes: jobj.msg,
                    timeout: 1500,
                    icon: 'success'
                });
        });
    }
}