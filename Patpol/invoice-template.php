<style>
    page {
        padding: 20 15 20 15;
        font-size: 8;
        font-family: LiberationSans;
    }

    td, th {
        padding: 0.5 1 0 1;
    }

    translation {
        <? if ($invoice['Invoice']['translation_language_id']): ?>
            font-size: 6;
            font-style: italic;
            display: inline;
        <? else: // w przypadku faktury w języku polskim tłumaczenia są ukryte ?>
            display: none;
        <? endif ?>
    }
</style>

<? foreach ((array)$parameters['pages'] as $page): ?>
    <page>
        <header height="85">
            <? if (isset($invoice['footerCreator'])): ?>
                <div position="absolute" x="12" y="267" font-size="6" text-align="center">
                    <?= $invoice['footerCreator']; ?>
                </div>
            <? endif; ?>

            <style>
                .invoice-header {
                    position: absolute;
                    y: -10;
                    text-align: center;
                }

                /* logo */
                .invoice-logo {
                    width: 45%;
                    height: 40;
                    float: left;
                }

                /* main info - start */
                .invoice-main-info {
                    width: 48%;
                    float: right;
                }

                .invoice-main-info tr th {
                    padding: 1.5 0 1.5 0;
                }
                .invoice-main-info tr td {
                    padding: 1 0 1 1;
                }

                .invoice-main-info tr:child(0) {
                    font-size: 9;
                    text-align: center;
                    background-gradient-type: linear;
                    background-gradient-color1: white;
                    background-gradient-color2: lightgrey;
                }

                .invoice-main-info tr td div:child(0) {
                    width: 49%;
                    float: left;
                }

                .invoice-main-info tr td div:child(1) {
                    width: 49%;
                    float: right;
                }

                .invoice-main-info tr td div div:child(0) {
                    width: 57%;
                    float: left;
                }

                .invoice-main-info tr td div div:child(1) {
                    width: 42%;
                    float: right;
                }
                /* main info - stop */

                /* transaction sides - start */
                <? if ($invoice['ContractorDetailReceiver']['id']): ?>
                    .invoice-transaction-side:child(0) {
                        width: 38%;
                        float: left;
                    }

                    .invoice-transaction-side:child(1) {
                        width: 31%;
                        float: left;
                    }

                    .invoice-transaction-side:child(2) {
                        width: 30%;
                        float: right;
                    }
                <? else: ?>
                    .invoice-transaction-side:child(0) {
                        width: 48%;
                        float: left;
                    }

                    .invoice-transaction-side:child(1) {
                        width: 48%;
                        float: right;
                    }
                <? endif ?>
                /* transaction sides - stop */

                .invoice-bar {
                    position: absolute;
                    y: 71;
                    font-size: 10;
                    text-align: right;
                    padding: 0 1 -0.5 0;
                    margin-top: 4;
                    border-bottom: 1px;
                    background-gradient-type: linear;
                    background-gradient-color1: white;
                    background-gradient-color2: lightgrey;
                    background-gradient-coordinates: 0 0 1 0;
                }
            </style>

            <? if ($invoice['Invoice']['header']): ?>
                <p class="invoice-header"><?= $xml->sanitize($invoice['Invoice']['header']); ?></p>
            <? endif ?>

            <div class="invoice-logo">
                <? if ($parameters['logo_path']): ?>
                    <img width="80" src="<?= $parameters['logo_path'] ?>"/>
                <? endif ?>
            </div>

            <div class="invoice-main-info">
                <table>
                    <tr>
                        <th>Faktura<?= $xml->sanitize($parameters['document_name_suffix']) ?> nr <?= $xml->sanitize($invoice['Invoice']['fullnumber']) ?></th>
                    </tr>
                    <tr>
                        <td>
                            <div>
                                <div>Data wystawienia:<br/><translation><?= $translation->get('Invoice:Data wystawienia') ?></translation></div>
                                <div><?= $xml->sanitize($invoice['Invoice']['date']) ?></div>
                                <div clear="both"/>
                            </div>

                            <div>
                                <? if (!$invoice['Invoice']['disposaldate_empty']): ?>
                                    <div>Data sprzedaży:<br/><translation><?= $translation->get('Invoice:Data sprzedaży') ?></translation></div>
                                    <div><?= $invoice['Invoice']['disposaldate'] ?></div>
                                <? endif ?>
                                <div clear="both"/>
                            </div>

                            <div clear="both"/>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <div>
                                <div>Termin płatności:<br/><translation><?= $translation->get('Invoice:Termin płatności') ?></translation></div>
                                <div><?= $xml->sanitize($invoice['Invoice']['paymentdate']) ?></div>
                                <div clear="both"/>
                            </div>

                            <div>
                                <div>Metoda płatności:<br/><translation><?= $translation->get('Invoice:Metoda płatności') ?></translation></div>
                                <div>
                                    <?= $xml->sanitize($invoice['Invoice']['paymentmethod']) ?><br/>
                                    <translation><?= $xml->sanitize($translation->get('Invoice:' . $invoice['Invoice']['paymentmethod'])) ?></translation>
                                </div>
                                <div clear="both"/>
                            </div>

                            <div clear="both"/>
                        </td>
                    </tr>
                </table>
            </div>

            <div clear="both"/>

            <? if ($parameters['duplicate']): ?>
                <div position="absolute" x="50" y="25">
                    <img width="40" src="/images/duplicate/<?= date("Y-m-d") ?>.png"/>
                </div>
            <? endif ?>

            <div class="invoice-transaction-side">
                <h3>Sprzedawca: <translation>(<?= $translation->get('Invoice:Sprzedawca') ?>)</translation></h3>
                <p><?= nl2br($xml->sanitize($invoice['Invoice']['seller_info'])) ?></p>
            </div>

            <div class="invoice-transaction-side">
                <h3>Nabywca: <translation>(<?= $translation->get('Invoice:Nabywca') ?>)</translation></h3>
                <p><?= nl2br($xml->sanitize($invoice['Invoice']['buyer_info'])) ?></p>
            </div>

            <? if ($invoice['ContractorDetailReceiver']['id']): ?>
                <div class="invoice-transaction-side">
                    <h3>Odbiorca: <translation>(<?= $translation->get('Invoice:Odbiorca') ?>)</translation></h3>
                    <p><?= nl2br($xml->sanitize($invoice['Invoice']['receiver_info'])) ?></p>
                </div>
            <? endif ?>

            <div clear="both"/>

            <div class="invoice-bar">
                <? if ($parameters['show_page_type']): ?>
                    <?= $page === 'original' ? 'O R Y G I N A Ł' : 'K O P I A' ?>
                <? else: ?>
                    <br/>
                <? endif ?>
            </div>
        </header>

        <body>
            <table>
                <header>
                    <tr>
                        <td width="3%">Lp<br/><translation><?= $translation->get('Invoice:Lp') ?></translation></td>
                        <td>Nazwa<br/><translation><?= $translation->get('Invoice:Nazwa') ?></translation></td>

                        <? if ($parameters['show_classification']): ?>
                            <td width="7%">PKWiU<br/><translation><?= $translation->get('Invoice:PKWiU') ?></translation></td>
                        <? endif ?>

                        <td width="6%">Jedn.<br/><translation><?= $translation->get('Invoice:Jedn.') ?></translation></td>
                        <td width="6%">Ilość<br/><translation><?= $translation->get('Invoice:Ilość') ?></translation></td>
                        <td width="11%">
                            <?= $invoice['Invoice']['tax_evaluation_method'] === 'netto' ? 'Cena netto' : 'Cena brutto' ?><br/>
                            <translation><?= $translation->get('Invoice:Cena') ?></translation>
                        </td>

                        <? if ($parameters['show_discount']): ?>
                            <td column-span="2">Rabat<br/><translation><?= $translation->get('Invoice:Rabat'); ?></translation></td>
                        <? endif ?>

                        <td width="8%">Stawka<br/><translation><?= $translation->get('Invoice:Stawka') ?></translation></td>
                        <td width="12%">Wartość netto<br/><translation><?= $translation->get('Invoice:Wartość netto') ?></translation></td>
                        <td width="12%">Wartość brutto<br/><translation><?= $translation->get('Invoice:Wartość brutto') ?></translation></td>
                    </tr>
                </header>

                <? foreach ((array)$invoiceContents as $key => $invoiceContent): ?>
                    <tr class="invoice-content">
                        <td><?= $key + 1 ?></td>
                        <td><?= $xml->sanitize($invoiceContent['InvoiceContent']['name']) ?></td>

                        <? if ($parameters['show_classification']): ?>
                            <td><?= $xml->sanitize($invoiceContent['InvoiceContent']['classification']) ?></td>
                        <? endif ?>

                        <td><?= $xml->sanitize($invoiceContent['InvoiceContent']['unit']) ?></td>
                        <td class="invoice-content-count"><?= $xml->niceFloat($invoiceContent['InvoiceContent']['count']) ?></td>
                        <td class="invoice-content-price"><?= $xml->currency($invoiceContent['InvoiceContent']['price']) ?></td>

                        <? if ($parameters['show_discount']): ?>
                            <td width="5%"><?= $xml->niceFloat($invoiceContent['InvoiceContent']['discount_percent']) ?>%</td>
                            <td width="9%"><?=
                                round(
                                    (
                                        (($xml->currency($invoiceContent['InvoiceContent']['price']) * $xml->niceFloat($invoiceContent['InvoiceContent']['count']))
                                        - ($xml->currency($invoiceContent['InvoiceContent']['discount_amount']))) / $xml->niceFloat($invoiceContent['InvoiceContent']['count'])
                                    ), 2)
                            ?></td>

                        <? endif ?>

                        <td><?= $invoiceContent['InvoiceContent']['vatcode'] ?></td>
                        <td class="invoice-content-netto"><?= $xml->currency($invoiceContent['InvoiceContent']['netto']) ?></td>
                        <td class="invoice-content-brutto"><?= $xml->currency($invoiceContent['InvoiceContent']['brutto']) ?></td>
                    </tr>
                <? endforeach ?>
            </table>
        </body>

        <footer height="80" margin-top="5">
            <style>
                /* vat sums - start */
                .invoice-sums {
                    float: left;
                    width: 50%;
                }
                .invoice-sums th {
                    font-style: normal;
                }
                /* vat sums - stop */


                /* payment info - start */
                .invoice-payment-info {
                    border:0;
                    float: right;
                    width: <?= $invoice['Invoice']['translation_language_id'] ? '40%' : '30%' ?>;
                }
                .invoice-payment-info tr td:child(1) {
                    text-align: right;
                }
                /* payment info - stop */


                /* annotations - start */
                .invoice-annotations {
                    float: left;
                    width: 48%;
                }
                .invoice-annotations p {
                    margin-top:3;
                }
                /* annotations - stop */

                .invoice-qr-code {
                    width: 25;
                    float: right;
                    text-align: center;
                    font-size: 6;
                }

                .invoice-additional-image {
                    float: right;
                    width: 70;
                }


                /* signatures - start */
                .invoice-signature {
                    text-align: center;
                    width: 45;
                }
                .invoice-signature:child(0) {
                    float: left;
                }
                .invoice-signature:child(1) {
                    float: right;
                }
                .invoice-signature span:child(0) {
                    font-style: bold;
                }
                .invoice-signature span:child(1) {
                    font-size: 7;
                }
                /* signatues - stop */

                .invoice-footer {
                    margin-top: 5;
                    text-align: center;
                }
            </style>

            <table class="invoice-sums">
                <tr>
                    <th width="25%">Stawka VAT<br/><translation><?= $translation->get('Invoice:Stawka VAT') ?></translation></th>
                    <th width="25%">Wartość Netto<br/><translation><?= $translation->get('Invoice:Wartość netto') ?></translation></th>
                    <th width="25%">Kwota VAT<br/><translation><?= $translation->get('Invoice:Kwota VAT') ?></translation></th>
                    <th width="25%">Wartość Brutto<br/><translation><?= $translation->get('Invoice:Wartość brutto') ?></translation></th>
                </tr>

                <? foreach ((array)$invoice['VatContent'] as $vatContent): ?>
                    <tr>
                        <td><?= $vatContent['VatCode']['label'] ?></td>
                        <td><?= $xml->currency($vatContent['netto']) ?></td>
                        <td><?= $xml->currency($vatContent['tax']) ?></td>
                        <td><?= $xml->currency($vatContent['brutto']) ?></td>
                    </tr>
                <? endforeach ?>

                <tr>
                    <td>Razem <translation>(<?= $translation->get('Invoice:Razem') ?>)</translation></td>
                    <td><?= $xml->currency($invoice['Invoice']['vat_content_netto']) ?></td>
                    <td><?= $xml->currency($invoice['Invoice']['vat_content_tax']) ?></td>
                    <td><?= $xml->currency($invoice['Invoice']['vat_content_brutto']) ?></td>
                </tr>
            </table>

            <table class="invoice-payment-info">
                <tr>
                    <td>Razem: <translation>(<?= $translation->get('Invoice:Razem') ?>)</translation></td>
                    <td><?= $xml->currency($invoice['Invoice']['total']) ?> <?= $invoice['Invoice']['currency'] ?></td>
                </tr>
                <tr>
                    <td width="60%">Zapłacono: <translation>(<?= $translation->get('Invoice:Zapłacono') ?>)</translation></td>
                    <td width="40%"><?= $xml->currency($invoice['Invoice']['alreadypaid_initial']) ?> <?= $invoice['Invoice']['currency'] ?></td>
                </tr>
                <tr>
                    <td>Pozostało: <translation>(<?= $translation->get('Invoice:Do zapłaty') ?>)</translation></td>
                    <td>
                        <?= $xml->currency($invoice['Invoice']['total'] - $invoice['Invoice']['alreadypaid_initial']) ?> <?= $invoice['Invoice']['currency'] ?>
                    </td>
                </tr>
            </table>

            <div clear="both"/>

            <div class="invoice-annotations">
                <? if ($invoice['Invoice']['legal_annotations']): ?>
                    <p><?= $xml->sanitize($invoice['Invoice']['legal_annotations']) ?></p>
                <? endif ?>

                <? if ($invoice['Invoice']['associated_document_annotations']): ?>
                    <?= $xml->sanitize($invoice['Invoice']['associated_document_annotations']) ?>
                <? endif ?>

                <? if ($invoice['Invoice']['description']): ?>
                    <p>
                        Uwagi: <translation>(<?= $translation->get('Invoice:Uwagi') ?>)</translation><br/>
                        <?= nl2br($invoice['Invoice']['description']) ?>
                    </p>
                <? endif ?>

                <? if ($invoice['Invoice']['currency'] !== 'PLN' && !$parameters['show_invoice_foreign_currency_vat_content']): ?>
                    <p>
                        1 <?= $invoice['Invoice']['currency'] ?> = <?= number_format($invoice['Invoice']['currency_exchange'], 4, ',', ' ') ?> PLN<br/>
                        Kurs z dnia: <?= $invoice['Invoice']['currency_date'] ?><br/>
                        Numer tabeli: <?= $invoice['Invoice']['currency_label'] ?>
                    </p>
                <? endif ?>
            </div>

            <? if ($parameters['invoice_send_external_type'] !== 'none'): ?>
                <div class="invoice-qr-code">
                    <a href="https://<?= $parameters['app_server'] ?>/faktura/<?= $invoice['Invoice']['company_id'] ?>/<?= $invoice['Invoice']['id'] ?>/<?= $invoice['Invoice']['hash'] ?>">
                        <qr-code>https://<?= $parameters['app_server'] ?>/faktura/<?= $invoice['Invoice']['company_id'] ?>/<?= $invoice['Invoice']['id'] ?>/<?= $invoice['Invoice']['hash'] ?></qr-code>
                        <br/>panel klienta
                    </a>
                </div>
            <? endif ?>
            <div clear="right"/>

            <? if ($parameters['additional_image_path']): ?>
                <div class="invoice-additional-image">
                    <img width="70" src="<?= $parameters['additional_image_path'] ?>"/>
                </div>
            <? endif ?>

            <div clear="both" margin-bottom="5"/>

            <? if ($parameters['signature_image_path']): ?>
                <img width="70" src="<?= $parameters['signature_image_path'] ?>"/>
            <? endif ?>


            <div margin-top="2">
                <div class="invoice-signature">
                    <span><?= $xml->sanitize($invoice['Invoice']['user_name']) ?></span><br/>
                    <span>Imię i nazwisko osoby uprawnionej do wystawiania faktury</span>
                </div>

                <div class="invoice-signature">
                    <span><br/></span><br/>
                    <span>Imię i nazwisko osoby uprawnionej do odbioru faktury</span>
                </div>

                <div clear="both"/>
            </div>

            <? if ($invoice['Invoice']['footer']): ?>
                <p class="invoice-footer"><?= $xml->sanitize($invoice['Invoice']['footer']); ?></p>
            <? endif ?>
        </footer>
    </page>

    <? if ($page === 'original' && $parameters['address']): // opcja adresu korespondecyjnego na odwrocie oryginału ?>
        <page padding-left="119" padding-top="255">
            <?= nl2br($xml->sanitize($invoice['Invoice']['buyeraddress'])); ?>
        </page>
    <? endif ?>
<? endforeach ?>

<? if ($parameters['leaflet']): // druczek przelewu ?>
    <page padding="20 0 0 20">
        <? for ($i = 0; $i < 2; $i++): ?>
            <div width="164" height="106" background-image="/images/reports/invoices/leaflet_<?= $i ?>.png">
                <style>
                    entry {
                        margin-left: 22.5;
                        margin-top: 4.6;
                    }
                </style>

                <entry><?= $xml->sanitize($invoice['CompanyDetail']['name']) ?></entry>
                <entry>
                    <?= $xml->sanitize($invoice['CompanyDetail']['street']) ?>
                    <?= $xml->sanitize(
                            $invoice['CompanyDetail']['building_number']
                            . ($invoice['CompanyDetail']['flat_number'] ? "/" . $xml->sanitize($invoice['CompanyDetail']['flat_number']) : "")
                        )
                    ?>,
                    <?= $xml->sanitize($invoice['CompanyDetail']['zip']) ?> <?= $xml->sanitize($invoice['CompanyDetail']['city']) ?>
                </entry>
                <entry letter-spacing="5"><?= str_replace(" ", "", $xml->sanitize($invoice['CompanyDetail']['bank_account'])) ?></entry>
                <entry margin-left="98"><?= str_replace(".", ",", $invoice['Invoice']['remaining']) ?></entry>
                <entry><br/></entry>
                <entry><?= $xml->sanitize($invoice['ContractorDetail']['name']) ?></entry>
                <entry>
                    <?= $xml->sanitize($invoice['ContractorDetail']['street']) ?>,
                    <?= $xml->sanitize($invoice['ContractorDetail']['zip']) ?> <?= $xml->sanitize($invoice['ContractorDetail']['city']) ?>
                </entry>
                <entry><?= $xml->sanitize($invoice['Invoice']['fullnumber']) ?></entry>
            </div>
        <? endfor ?>
    </page>
<? endif ?>
