/// <reference path="../../2012.1.214/jquery-1.7.1.min.js" />
(function ($) {
    $.fn.extend({

        AutoHeight: function () {
            var element = this.selector; 
            $(element + ' .t-grid-content').height($(element).parent().height() - ($(element + ' .t-toolbar').height() + $(element + ' .t-grid-header').height() + $(element + ' .t-grid-pager').height() + parseInt($(element + ' .t-grid-header').css('padding-left'))));
        },
        GetValuesOfTextBox: function () {
            var element = $(this[0]).val();

            return {
                Array: function () { return element.split(','); }
            }
        },
        SelectedOfGrid: function CountSelected() {
            var element = $(this[0]);
            var arrayOfCheckBox = element.find('input:checkbox');
            var arrayOfValue = 0;
            arrayOfCheckBox.each(function (index, domEle) {
                if ($(domEle).val() != "-1")
                    arrayOfValue += $(domEle).attr("checked") ? 1 : 0;
                return arrayOfValue;
            });

            return {
                Count: function () { return arrayOfValue; },

                ArrayOfVal: function () { return arrayOfValue; },

                SetIdToBox: function (id) {
                    (FindIndexOfArray(id, false) == null) ? element.val(element.val() + id + ",") : null;
                },
                DelIdOfBox: function (id) {
                    element.val(FindIndexOfArray(id, true));

                }

            }

        },
        CountCheckBoxOfGrid: function () {
            return $(this[0]).data('tGrid').$tbody.find('TR');
        },
        CheckAllSelected: function () {
            var grid = $(this[0]);
        },
        CheckAllBoxOfGridForTrue: function () {
            var checkBoxes = $(this[0]).data('tGrid').$tbody.find('INPUT');
            for (var i = 0; i < checkBoxes.length; ++i) {
                if (!$(checkBoxes[i]).attr('checked'))
                    return false;
            }
            return true;
        },
        GetCheckAllBoxOnHead: function () {
            var checkBoxAll = $(this[0]).data('tGrid').$header.find('INPUT');

            return {
                Get: function () {
                    return checkBoxAll;
                },

                SetChecked: function (bool) {
                    $(checkBoxAll).attr('checked', bool);
                }
            }

        },
        SaveOnTextBoxChecked: function () {
            var element = $(this[0]);
            //element.val("");
            if (element.attr('type') == 'hidden') {
                var arrayOfValue = element.val();
            }

            return {
                Save: function (assetIdes) {
                    if (assetIdes.length != 0) {
                        assetIdes.each(function (index, domEle) {
                            if ($(domEle).attr("checked"))
                                if ($(domEle).val() != "-1")
                                    if (arrayOfValue.indexOf("," + $(domEle).val() + ",") == -1)
                                        arrayOfValue += $(domEle).val() + ',';
                            if (!$(domEle).attr("checked"))
                                if ($(domEle).val() != "-1")
                                    arrayOfValue = arrayOfValue.replace("," + $(domEle).val() + ",", ",");
                            return arrayOfValue;
                        });

                    }
                    element.val(arrayOfValue);
                }

            }
        },

        AddOnTextBoxChecked: function (value) {
            var element = $(this[0]);
            element.val(element.val() + value + ",");
            return true;
        },

        RemoveFromTextBoxChecked: function (value) {
            var element = $(this[0]);
            element.val(element.val().replace("," + value + ",", ","));
            return true;
        },

        CountCheckedFromTextBox: function (value) {
            var element = $(this[0]).val();
            var checkedArray = element.split(',');
            var arrayOfValue = 0;
            jQuery.each(checkedArray, function () {
                if (this != "")
                    arrayOfValue += 1;
                return arrayOfValue;
            });

            return arrayOfValue;
        }




    });
})(jQuery);


function OnChekBox(e) {

    var assetGrid = $('#plotchart_geometries input:checkbox');
    //$('#SelectedAssetsId').SaveOnTextBoxChecked().Save(assetGrid);
    if ($(e).attr('checked'))
        $('#SelectedPlotsId').AddOnTextBoxChecked($(e).val());
    else
        $('#SelectedPlotsId').RemoveFromTextBoxChecked($(e).val());
    //CountSelected();
    //ActiveButton();
}


function OnChekBoxAll(e) {
    var assetGrid = $(e).attr("checked") ? $('#plotchart_geometries input:checkbox').attr("checked", true) : $('#plotchart_geometries input:checkbox').attr("checked", false);
    $('#SelectedPlotsId').SaveOnTextBoxChecked().Save(assetGrid);
    //CountSelected();
    //ActiveButton();
}

function CountSelected() {
    //$('#plotchart_geometries').ToolBarOfGrid().WriteCountSelected($('#SelectedAssetsId').CountCheckedFromTextBox());
}

function ClearAll() {
    $("#SelectedPlotsId").val(",");
    CountSelected();
    // ActiveButton();
    $('#plotchart_geometries input:checkbox').attr("checked", false);
}

function PlotonRowSelect(e) {

    var CH = $(e.row.cells[0]).children();
    if (CH.attr('checked'))
        CH.attr('checked', false);
    else
        CH.attr('checked', true);

    OnChekBox(CH);
    //ActiveButton();
}

function PlotOnRowDataBound(e) {

    var PID = $("#SelectedPlotsId").val().split(',');
    $.each(PID, function (k, v) {
        if (parseInt(v) > 0)
            if (e.dataItem.Id == v)
                $(e.row).find('INPUT').attr('checked', true);
    });

    $('#plotchart_geometries').CheckAllBoxOfGridForTrue() ? $('#plotchart_geometries').GetCheckAllBoxOnHead().SetChecked(true) : $('#plotchart_geometries').GetCheckAllBoxOnHead().SetChecked(false);

}