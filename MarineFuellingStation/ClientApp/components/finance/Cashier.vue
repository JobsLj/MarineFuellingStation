﻿<template>
    <div id="root">
        <yd-tab :callback="change">
            <yd-tab-panel label="待结算">
                <yd-cell-group>
                    <yd-search v-model="sv1" />
                    <yd-infinitescroll :callback="loadList" ref="orderinfinitescroll1">
                        <yd-cell-item slot="list" arrow @click.native="orderclick(o)" v-for="o in readypayorders" :key="o.id">
                            <div slot="left">
                                <p class="font16">{{o.carNo}}</p>
                                <p class="font14 col-light-gray">{{o.name}}</p>
                                <p class="col-light-gray" v-show="o.client != null">{{strCompanyName(o)}}</p>
                            </div>
                            <div slot="right" class="align-right lineheight24 cell-padding">
                                <p class="col-gray font16">￥{{o.totalMoney}}</p>
                                <p class="col-light-gray font14">{{o.product.name}} / {{o.count}}{{o.unit}} / {{o.price}}</p>
                                <p class="col-coral" v-if="o.client && o.client.balances && o.client.balances != 0">个人余额：￥{{o.client.balances}}</p>
                                <p class="col-coral" v-if="o.client && o.client.company && o.client.company.balances">公司余额：￥{{o.client.company == null ? 0 : o.client.company.balances}}</p>
                            </div>
                        </yd-cell-item>

                        <!-- 数据全部加载完毕显示 -->
                        <span slot="doneTip">没有数据啦~</span>
                        <!-- 加载中提示，不指定，将显示默认加载中图标 -->
                        <img slot="loadingTip" src="http://static.ydcss.com/uploads/ydui/loading/loading10.svg" />
                    </yd-infinitescroll>
                </yd-cell-group>
            </yd-tab-panel>
            <yd-tab-panel label="已结算">
                <yd-cell-group>
                    <yd-search v-model="sv2" />
                    <yd-infinitescroll :callback="loadList" ref="orderinfinitescroll2">
                        <yd-cell-item slot="list" arrow v-for="o in haspayorders" :key="o.id" @click.native="showMenusclick(o)">
                            <div slot="left">
                                <p class="font16">{{o.carNo}}</p>
                                <p class="col-light-gray font14">{{o.name}}</p>
                            </div>
                            <div slot="right" class="align-right lineheight24" style="margin:10px 5px 10px 0px">
                                <p class="col-gray font16">￥{{o.totalMoney}}</p>
                                <p class="col-light-gray font14">{{o.product.name}} / {{o.count}}{{o.unit}} / {{o.price}}</p>
                            </div>
                        </yd-cell-item>

                        <!-- 数据全部加载完毕显示 -->
                        <span slot="doneTip">没有数据啦~</span>

                        <!-- 加载中提示，不指定，将显示默认加载中图标 -->
                        <img slot="loadingTip" src="http://static.ydcss.com/uploads/ydui/loading/loading10.svg" />
                    </yd-infinitescroll>
                </yd-cell-group>
            </yd-tab-panel>
            <yd-tab-panel label="挂账">
                <yd-cell-group>
                    <yd-search v-model="sv3" />
                    <yd-infinitescroll :callback="loadList" ref="orderinfinitescroll3">
                        <yd-cell-item slot="list" arrow v-for="o in nopayorders" :key="o.id" @click.native="showMenusclick(o)">
                            <div slot="left">
                                <p class="font16">{{o.carNo}}</p>
                                <p class="col-light-gray font14">{{o.name}}</p>
                            </div>
                            <div slot="right" class="align-right lineheight24" style="margin:10px 5px 10px 0px">
                                <p class="col-gray font14">￥{{o.totalMoney}}</p>
                                <p class="col-green font16">{{getDiffDate(o.createdAt, 'hour')}}</p>
                                <p class="col-light-gray font14">{{o.product.name}} / {{o.count}}{{o.unit}} / {{o.price}}</p>
                            </div>
                        </yd-cell-item>

                        <!-- 数据全部加载完毕显示 -->
                        <span slot="doneTip">没有数据啦~</span>

                        <!-- 加载中提示，不指定，将显示默认加载中图标 -->
                        <img slot="loadingTip" src="http://static.ydcss.com/uploads/ydui/loading/loading10.svg" />
                    </yd-infinitescroll>
                </yd-cell-group>
            </yd-tab-panel>
        </yd-tab>
        <yd-popup v-model="showPayTypes" position="right" width="70%">
            <yd-cell-group title="第一步：结账方式" v-show="lastshow">
                <yd-cell-item type="checkbox">
                    <span slot="left">现金</span>
                    <input slot="right" type="checkbox" value="0" v-model="orderPayTypes" />
                </yd-cell-item>

                <yd-cell-item type="checkbox">
                    <span slot="left">微信</span>
                    <input slot="right" type="checkbox" value="1" v-model="orderPayTypes" />
                </yd-cell-item>

                <yd-cell-item type="checkbox">
                    <span slot="left">支付宝</span>
                    <input slot="right" type="checkbox" value="2" v-model="orderPayTypes" />
                </yd-cell-item>

                <yd-cell-item type="checkbox">
                    <span slot="left">桂行刷卡</span>
                    <input slot="right" type="checkbox" value="3" v-model="orderPayTypes" />
                </yd-cell-item>

                <yd-cell-item type="checkbox">
                    <span slot="left">工行刷卡</span>
                    <input slot="right" type="checkbox" value="4" v-model="orderPayTypes" />
                </yd-cell-item>

                <!--暂时取消-->
                <!--<yd-cell-item type="checkbox">
                    <span slot="left">刷卡三</span>
                    <input slot="right" type="checkbox" value="5" v-model="orderPayTypes" />
                </yd-cell-item>-->

                <yd-cell-item type="checkbox" v-show="selectedOrder.client != null">
                    <div slot="left">
                        <p> 账户余额</p>
                        <p class="col-red">￥{{selectedOrder.client == null? 0 : selectedOrder.client.balances}}</p>
                    </div>
                    <input slot="right" type="checkbox" value="6" v-model="orderPayTypes" />
                </yd-cell-item>

                <yd-cell-item type="checkbox" v-show="selectedOrder.client && selectedOrder.client.company">
                    <div slot="left">
                        <p> 公司账户余额</p>
                        <p class="col-red">￥{{selectedOrder.client && selectedOrder.client.company? selectedOrder.client.company.balances : 0}}</p>
                    </div>
                    <input slot="right" type="checkbox" value="7" v-model="orderPayTypes" />
                </yd-cell-item>

            </yd-cell-group>
            <yd-cell-group title="第二步：结算金额" v-show="!lastshow">
                <yd-cell-item v-show="!lastshow">
                    <span slot="left" class="col-red">应收：</span>
                    <span slot="right" class="col-red font16">￥{{selectedOrder.totalMoney}}元</span>
                </yd-cell-item>
                <yd-cell-item v-show="!lastshow">
                    <span slot="left">实收：</span>
                    <span slot="right">￥{{getTotalPayMoney()}}元</span>
                </yd-cell-item>
                <yd-cell-item v-show="!lastshow">
                    <span slot="left">找零：</span>
                    <span slot="right">￥{{payInfact - selectedOrder.totalMoney}}元</span>
                </yd-cell-item>
            </yd-cell-group>
            <yd-cell-group title="输入金额" v-show="!lastshow">
                <yd-cell-item v-show="showInputs[0]">
                    <span slot="left">现金：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[0]"></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
                <yd-cell-item v-show="showInputs[1]">
                    <span slot="left">微信：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[1]"></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
                <yd-cell-item v-show="showInputs[2]">
                    <span slot="left">支付宝：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[2]"></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
                <yd-cell-item v-show="showInputs[3]">
                    <span slot="left">桂行刷卡：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[3]"></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
                <yd-cell-item v-show="showInputs[4]">
                    <span slot="left">工行刷卡：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[4]" placeholder=""></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
                <yd-cell-item v-show="showInputs[6]">
                    <span slot="left">账户余额：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[6]" placeholder=""></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
                <yd-cell-item v-show="showInputs[7]">
                    <span slot="left">公司账户余额：</span>
                    <yd-input slot="right" v-model="orderPayMoneys[7]" placeholder=""></yd-input>
                    <span slot="right">元</span>
                </yd-cell-item>
            </yd-cell-group>
            <div class="align-center">
                <yd-button size="large" type="warning" @click.native="nextclick()" v-show="lastshow">下一步</yd-button>
            </div>
            <div class="align-center">
                <yd-button size="large" type="warning" @click.native="lastclick()" v-show="!lastshow">上一步</yd-button>
            </div>
            <div class="align-center">
                <yd-button size="large" type="primary" @click.native="validateMoney()" v-show="!lastshow" :disabled ="payInfact - selectedOrder.totalMoney < 0">结账</yd-button>
            </div>
        </yd-popup>
        <!--popup充值-->
        <yd-popup v-model="showCharge" position="right" width="70%">
            <yd-cell-group title="充值">
                <div class="align-center font-rem4" style="padding: .2rem 0;">当前余额：￥{{strBalances()}}</div>
                <yd-cell-item>
                    <span slot="left">当前账户：</span>
                    <span slot="right" class="col-green">{{chargeAccount}}</span>
                </yd-cell-item>
                <yd-cell-item arrow type="label">
                    <span slot="left">支付方式：</span>
                    <select slot="right" v-model="chargeLog.payType">
                        <option value="0">现金</option>
                        <option value="1">微信</option>
                        <option value="2">支付宝</option>
                        <option value="3">桂行刷卡</option>
                        <option value="4">工行刷卡</option>
                    </select>
                </yd-cell-item>
                <yd-cell-item>
                    <span slot="left">充值金额：</span>
                    <yd-input slot="right" type="number" v-model="chargeLog.money"></yd-input>
                </yd-cell-item>
            </yd-cell-group>
            <yd-button size="large" type="primary" @click.native="postCharge()" :disabled="chargeLog.money <= 0">提交</yd-button>
        </yd-popup>
        <!--popup付款金额和方式记录-->
        <yd-popup v-model="showPayments" position="right" width="50%">
            <yd-cell-group title="付款金额和方式">
                <yd-cell-item type="label" v-for="p in orderPayments" :key="p.id">
                    <span slot="left">{{strOrderPayType(p.payTypeId)}}</span>
                    <span slot="right">￥{{p.money}}</span>
                </yd-cell-item>
            </yd-cell-group>
            <div style="text-align: right;padding-right:.2rem">找零：￥{{selectedOrder.totalMoney - totalPayMoney}}</div>
        </yd-popup>
        <!--actionsheet-->
        <yd-actionsheet :items="actItems" v-model="showAct" cancel="取消"></yd-actionsheet>
        <yd-actionsheet :items="menus" v-model="showMenus" cancel="取消"></yd-actionsheet>
    </div>
</template>

<script src="./cashier.ts" />

