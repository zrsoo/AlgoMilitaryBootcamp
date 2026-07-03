public class SnapshotArray {
    List<(int snapId, int val)>[] arr;
    int snap;

    public SnapshotArray(int length) {
        arr = new List<(int snapId, int val)>[length];

        for(int i = 0; i < length; ++i)
            arr[i] = new();

        snap = 0;
    }
    
    public void Set(int index, int val) {
        var lst = arr[index];

        if(lst.Count > 0 && lst[lst.Count - 1].snapId == this.snap) lst[lst.Count - 1] = (snap, val);
        else lst.Add((snap, val));
    }
    
    public int Snap() {
        snap++;
        return snap - 1;
    }
    
    public int Get(int index, int snap_id) {
        var list = arr[index];

        if(list.Count == 0) return 0;

        int lo = 0, hi = list.Count;
        while(lo < hi)
        {
            int mid = (lo + hi) / 2;

            if(list[mid].snapId == snap_id) return list[mid].val;

            if(list[mid].snapId <= snap_id)
                lo = mid + 1;
            else hi = mid;
        }

        return lo > 0 ? list[lo - 1].val : 0;
    }
}

/**
 * Your SnapshotArray object will be instantiated and called as such:
 * SnapshotArray obj = new SnapshotArray(length);
 * obj.Set(index,val);
 * int param_2 = obj.Snap();
 * int param_3 = obj.Get(index,snap_id);
 */
