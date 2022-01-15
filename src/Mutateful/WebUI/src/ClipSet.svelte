<script lang="ts">
    import ClipSlot from "./ClipSlot.svelte";
    let clips = [
        [{formula:"", clipRef:"A1"}, {formula:"", clipRef:"B1"}, {formula:"", clipRef:"C1"}, {formula:"", clipRef:"D1"}, {formula:"", clipRef:"E1"}, {formula:"", clipRef:"F1"}, {formula:"", clipRef:"G1"}, {formula:"", clipRef:"H1"}, {formula:"", clipRef:"I1"}, {formula:"", clipRef:"J1"}],
        [{formula:"", clipRef:"A2"}, {formula:"", clipRef:"B2"}, {formula:"", clipRef:"C2"}, {formula:"", clipRef:"D2"}, {formula:"", clipRef:"E2"}, {formula:"", clipRef:"F1"}, {formula:"", clipRef:"G1"}, {formula:"", clipRef:"H1"}, {formula:"", clipRef:"I1"}, {formula:"", clipRef:"J1"}],
        [{formula:"", clipRef:"A3"}, {formula:"", clipRef:"B3"}, {formula:"", clipRef:"C3"}, {formula:"", clipRef:"D3"}, {formula:"", clipRef:"E3"}, {formula:"", clipRef:"F1"}, {formula:"", clipRef:"G1"}, {formula:"", clipRef:"H1"}, {formula:"", clipRef:"I1"}, {formula:"", clipRef:"J1"}],
        [{formula:"", clipRef:"A4"}, {formula:"", clipRef:"B4"}, {formula:"", clipRef:"C4"}, {formula:"", clipRef:"D4"}, {formula:"", clipRef:"E4"}, {formula:"", clipRef:"F1"}, {formula:"", clipRef:"G1"}, {formula:"", clipRef:"H1"}, {formula:"", clipRef:"I1"}, {formula:"", clipRef:"J1"}]
    ];
</script>

<table class="main-cell-table">
    {#each clips as clipRow}
        <tr>
        {#each clipRow as clip}
            <td><ClipSlot clipRef="{clip.clipRef}" formula="{clip.formula}"/></td>
        {/each}
        </tr>
    {/each}
</table>

<style>
    table {
        border-collapse: collapse;
        table-layout: fixed;
        width: 100%;
    }
    th, td {
        width: 150px;
        overflow: hidden;
        white-space: nowrap;
    }
</style>