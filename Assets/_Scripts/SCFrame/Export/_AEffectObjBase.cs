namespace SCFrame.RefData
{
    /// <summary>
    /// 配表效果对象 需要解析使用 用在一个集合体的情况
    /// 如配置一个buff效果 写一个类继承此抽象类 有id 持续回合等字段
    /// </summary>
    public abstract class _AEffectObjBase
    {
        public virtual void Deserialize(string _str)
        {
            OnDeserialize(_str);
        }
        protected abstract void OnDeserialize(string _str);
    }
}
